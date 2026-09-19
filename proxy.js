const http = require('http');

const TARGET_HOST = '127.0.0.1';
const TARGET_PORT = 5001;
const LISTEN_PORT = 5000;

function deepNormalize(obj) {
  if (obj === null || obj === undefined || typeof obj !== 'object') return obj;
  if (Array.isArray(obj)) return obj.map(deepNormalize);

  const out = {};
  for (const key of Object.keys(obj)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1);
    const val = deepNormalize(obj[key]);
    out[camel] = val;
    out[key] = val;
  }
  return out;
}

function transformForMobile(url, json) {
  if (!json || typeof json !== 'object') return json;

  // If response has { success: ..., data: ... }
  if ('data' in json) {
    if (Array.isArray(json.data)) {
      const normArr = json.data.map(deepNormalize);
      json.data = normArr;
      // If contract endpoint, also copy first contract properties to top-level for direct access
      if (url.includes('/api/me/contract') && normArr.length > 0) {
        Object.assign(json, normArr[0]);
      }
    } else if (json.data && typeof json.data === 'object') {
      const normObj = deepNormalize(json.data);
      json.data = normObj;
      // Merge all properties onto top-level json so both json.xxx and json.data.xxx work
      Object.assign(json, normObj);
    }
  }

  return deepNormalize(json);
}

const server = http.createServer((req, res) => {
  // CORS support
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', '*');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const options = {
    hostname: TARGET_HOST,
    port: TARGET_PORT,
    path: req.url,
    method: req.method,
    headers: {
      ...req.headers,
      host: `localhost:${TARGET_PORT}`,
    },
  };

  const proxyReq = http.request(options, (proxyRes) => {
    const contentType = proxyRes.headers['content-type'] || '';
    const isJson = contentType.includes('application/json');

    if (!isJson) {
      const headers = { ...proxyRes.headers };
      headers['access-control-allow-origin'] = '*';
      res.writeHead(proxyRes.statusCode, headers);
      proxyRes.pipe(res, { end: true });
      return;
    }

    const chunks = [];
    proxyRes.on('data', (chunk) => chunks.push(chunk));
    proxyRes.on('end', () => {
      const rawBody = Buffer.concat(chunks).toString('utf-8');
      try {
        let parsed = JSON.parse(rawBody);
        parsed = transformForMobile(req.url, parsed);
        const transformed = Buffer.from(JSON.stringify(parsed), 'utf-8');

        const headers = { ...proxyRes.headers };
        headers['access-control-allow-origin'] = '*';
        headers['access-control-allow-methods'] = 'GET, POST, PUT, DELETE, OPTIONS';
        headers['access-control-allow-headers'] = '*';
        headers['content-length'] = transformed.length;
        delete headers['content-encoding']; // in case IIS compressed

        res.writeHead(proxyRes.statusCode, headers);
        res.end(transformed);
      } catch (err) {
        const rawBuf = Buffer.concat(chunks);
        const headers = { ...proxyRes.headers };
        headers['access-control-allow-origin'] = '*';
        res.writeHead(proxyRes.statusCode, headers);
        res.end(rawBuf);
      }
    });
  });

  proxyReq.on('error', (err) => {
    console.error(`[Proxy Error] ${req.method} ${req.url} -> ${err.message}`);
    if (!res.headersSent) {
      res.writeHead(502, { 'Content-Type': 'application/json; charset=utf-8' });
      res.end(JSON.stringify({ Message: 'Proxy cannot reach IIS Express at localhost:5001: ' + err.message }));
    }
  });

  req.pipe(proxyReq, { end: true });
});

server.listen(LISTEN_PORT, '0.0.0.0', () => {
  console.log(`[Reverse Proxy v2] Listening on 0.0.0.0:${LISTEN_PORT} -> http://${TARGET_HOST}:${TARGET_PORT}`);
});
