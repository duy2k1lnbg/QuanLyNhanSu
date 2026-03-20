using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Servies
{
    public class VectorItem
    {
        public string Text { get; set; }
        public float[] Vec { get; set; }
        public string Tag { get; set; } // 🔥 phân loại
    }

    public class VectorService
    {
        private List<VectorItem> _data;

        private const int DIM = 128;
        private const float THRESHOLD = 0.75f; // 🔥 lọc kết quả rác
        private const int TOPK = 5;

        public VectorService()
        {
            _data = new List<VectorItem>();
        }

        // ================= EMBEDDING =================
        private float[] Embed(string text)
        {
            var vec = new float[DIM];

            for (int i = 0; i < text.Length && i < DIM; i++)
            {
                vec[i] = (float)text[i] / 255;
            }

            return Normalize(vec);
        }

        // ================= ADD =================
        public void Add(string text, string tag = "GENERAL")
        {
            _data.Add(new VectorItem
            {
                Text = text,
                Vec = Embed(text),
                Tag = tag
            });
        }

        // ================= SEARCH =================
        public List<string> Search(string query, string tag = null)
        {
            var q = Embed(query);

            var result = _data
                .Where(x => tag == null || x.Tag == tag) // 🔥 filter theo loại
                .Select(x => new
                {
                    x.Text,
                    Score = Cosine(q, x.Vec)
                })
                .Where(x => x.Score >= THRESHOLD) // 🔥 lọc rác
                .OrderByDescending(x => x.Score)
                .Take(TOPK)
                .Select(x => x.Text)
                .ToList();

            return result;
        }

        // ================= COSINE =================
        private float Cosine(float[] a, float[] b)
        {
            float dot = 0;

            for (int i = 0; i < DIM; i++)
            {
                dot += a[i] * b[i];
            }

            return dot; // vì đã normalize
        }

        // ================= NORMALIZE =================
        private float[] Normalize(float[] v)
        {
            float norm = (float)Math.Sqrt(v.Sum(x => x * x)) + 1e-6f;

            for (int i = 0; i < v.Length; i++)
            {
                v[i] /= norm;
            }

            return v;
        }
    }
}