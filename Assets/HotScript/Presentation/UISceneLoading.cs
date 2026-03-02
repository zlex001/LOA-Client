using Framework;
using Game.Basic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation
{

    public class UISceneLoading : UI.Core
    {
        //????
        public Text ui_progressText;
        //??????
        public Image ui_loadingProgress;

        private float m_ProgressPercent = 0.0f;

        public override void OnCreate(params object[] args)
        {
            SetActive(false);
            Game.Basic.Event.Instance.Add("UI_Show_SceneLoading", OnShowLoading);
            Game.Basic.Event.Instance.Add("UI_Hide_SceneLoading", OnHideLoading);
        }

        public override void OnClose()
        {
            base.OnClose();

            Game.Basic.Event.Instance.Remove("UI_Show_SceneLoading", OnShowLoading);
            Game.Basic.Event.Instance.Remove("UI_Hide_SceneLoading", OnHideLoading);
        }

        /// <summary>
        /// ???Loading
        /// </summary>
        public void OnShowLoading(params object[]args)
        {
            SetActive(true);

            m_ProgressPercent = 0.0f;

            ui_progressText.text = "";
            ui_loadingProgress.fillAmount = 0;

            StartCoroutine(LoadProgressIEnumerator());
        }

        public void OnHideLoading(params object[] args)
        {
            SetActive(false);
        }


        IEnumerator LoadProgressIEnumerator()
        {
            while (true)
            {
                m_ProgressPercent += 0.01f;

                if (m_ProgressPercent >= 1)
                {
                    OnHideLoading();
                    break;
                }
                else
                {
                    float percent = m_ProgressPercent * 100;
                    ui_progressText.text = Math.Ceiling(percent) + "%";
                    ui_loadingProgress.fillAmount = m_ProgressPercent;
                }
                yield return new WaitForSeconds(0.01f);
            }

            yield return null;
        }

    }
}