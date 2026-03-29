using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services
{
    public class VectorItem
    {
        public string Text { get; set; }
        public float[] Vec { get; set; }
        public string Tag { get; set; }
    }

    public class VectorService
    {
        private List<VectorItem> _data = new List<VectorItem>();
        private const int DIM = 128;
        private const float THRESHOLD = 0.75f;
        private const int TOPK = 5;

        private float[] Embed(string text)
        {
            var vec = new float[DIM];
            text = text.ToLower();

            for (int i = 0; i < text.Length - 1 && i < DIM; i++)
            {
                int charPairHash = (text[i] << 5) ^ text[i + 1];
                vec[i] = (float)(charPairHash % 255) / 255f;
            }

            return Normalize(vec);
        }

        private float[] Normalize(float[] v)
        {
            float norm = (float)Math.Sqrt(v.Sum(x => x * x)) + 1e-6f;
            for (int i = 0; i < v.Length; i++) v[i] /= norm;
            return v;
        }

        private float Cosine(float[] a, float[] b)
        {
            float dot = 0;
            for (int i = 0; i < DIM; i++) dot += a[i] * b[i];
            return dot;
        }

        public void Add(string text, string tag = "GENERAL")
        {
            _data.Add(new VectorItem { Text = text, Vec = Embed(text), Tag = tag });
        }

        public List<string> Search(string query, string tag = null)
        {
            var q = Embed(query);

            return _data
                .Where(x => tag == null || x.Tag == tag)
                .Select(x => new { x.Text, Score = Cosine(q, x.Vec) })
                .Where(x => x.Score >= THRESHOLD)
                .OrderByDescending(x => x.Score)
                .Take(TOPK)
                .Select(x => x.Text)
                .ToList();
        }
    }
}