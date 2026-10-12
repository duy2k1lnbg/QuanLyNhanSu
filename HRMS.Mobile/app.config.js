const fs = require('node:fs');
const path = require('node:path');

module.exports = ({config}) => {
  const projectId = process.env.EXPO_PUBLIC_EAS_PROJECT_ID || config.extra?.eas?.projectId;
  const googleServicesFile = process.env.GOOGLE_SERVICES_JSON || config.android?.googleServicesFile;
  const requirePush = process.env.HRMS_REQUIRE_REMOTE_PUSH === 'true';
  if (projectId && !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(projectId)) {
    throw new Error('EAS projectId must be the UUID of the HRMS Expo project.');
  }
  if (requirePush && (!projectId || !googleServicesFile)) {
    throw new Error('Push build requires EXPO_PUBLIC_EAS_PROJECT_ID and GOOGLE_SERVICES_JSON. Configure Firebase/EAS first.');
  }
  if (googleServicesFile) {
    const file = path.resolve(__dirname, googleServicesFile);
    if (!fs.existsSync(file)) throw new Error('GOOGLE_SERVICES_JSON file does not exist.');
    const data = JSON.parse(fs.readFileSync(file, 'utf8'));
    if (!data.client?.some(client => client.client_info?.android_client_info?.package_name === config.android?.package)) {
      throw new Error('Firebase Android package must match com.qlns.hrmsmobile.');
    }
  }
  return {
    ...config,
    android: {...config.android, ...(googleServicesFile ? {googleServicesFile} : {})},
    extra: {...config.extra, ...(projectId ? {eas:{...config.extra?.eas, projectId}} : {})},
  };
};
