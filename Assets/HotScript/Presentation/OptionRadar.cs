using Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class OptionRadar : OptionItem
    {
        public Image ui_radarImg;
        public Text[] ui_numTexts, ui_titleTexts;
        public Dictionary<string, int> data;
        private RectTransform scrollViewRect;
        void Start()
        {
            if (data != null)
            {
                Refresh(6);
            }
        }
   
        public void SetScrollView(RectTransform scrollViewRect) 
        {
            this.scrollViewRect = scrollViewRect;
        }
        public  void Refresh(float size)
        {
            for (int i = 0; i < data.Count; i++)
            {
                var d = data.ElementAt(i);
                ui_titleTexts[i].text = d.Key;
                ui_numTexts[i].text = d.Value.ToString();
            }
            Material material = ui_radarImg.GetComponent<Image>().material;
            material.SetFloat("_StencilComp", 3);
            material.SetFloat("_Stencil", 1);
            material.SetFloat("_StencilWriteMask", 0);
            material.SetFloat("_StencilReadMask", 1);
            Texture texture = ui_radarImg.GetComponent<Image>().sprite.texture;
            CanvasRenderer render = ui_radarImg.GetComponent<CanvasRenderer>();
            float radarSize = size;

            if (data.Count >= 5)
            {
                int _length = data.Count;
                Mesh mesh = new Mesh();
                Vector3[] vertices = new Vector3[_length + 1];
                Vector2[] uv = new Vector2[_length + 1];
                int[] triangles = new int[3 * _length];
                float angleIncrement = 360f / _length;

                vertices[0] = Vector3.zero;
                for (int i = 1; i <= _length; i++)
                {
                    vertices[i] = Quaternion.Euler(0, 0, -angleIncrement * (i - 1)) * Vector3.up * radarSize * (data.ElementAt(i - 1).Value+1);
                }
                uv[0] = Vector2.zero;
                for (int i = 1; i <= _length; i++)
                {
                    uv[i] = Vector2.one;
                }
                for (int i = 0; i < _length * 3; i++)
                {
                    if (i % 3 == 0)
                    {
                        triangles[i] = 0;
                    }
                    else
                    {
                        if ((i - 1) % 3 == 0)
                        {
                            triangles[i] = (i - 1) / 3 + 1;
                        }
                        else
                        {
                        triangles[i] = (i - 2) / 3 + 2;
                    }
                }
                if (i == _length * 3 - 1)
                    {
                        triangles[i] = 1;
                    }
                }
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.triangles = triangles;
                render.SetMesh(mesh);
                render.SetMaterial(material, texture);
            }
        }
   

   

    }
}