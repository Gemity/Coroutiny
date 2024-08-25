using Gemity.Coroutiny;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = Gemity.Api.Debug;

namespace Gemity.Coroutiny
{
    public class CoroutinyExcuter : MonoBehaviour
    {
        private static CoroutinyExcuter _instance;
        public static CoroutinyExcuter Instance =>_instance;

        private void Awake()
        {
            if(_instance == null)
                _instance = this;
            else
                DontDestroyOnLoad(this);
        }

        private void Update()
        {
            foreach (var i in Coroutiny._allCoroutiny)
            {
                i.Update();
            }
        }
    }

    public static class CoroutinyExtension
    {
        public static Coroutiny CreateCoroutiny(this MonoBehaviour mono, IEnumerator ie, bool selfGo = true)
        {
            var routiny = Coroutiny.Create(ie, selfGo ? mono : CoroutinyExcuter.Instance);
            Coroutiny._allCoroutiny.Add(routiny);
            return routiny;
        }

        public static Coroutiny StartCoroutiny(this MonoBehaviour mono, IEnumerator ie, bool selfGo = true)
        {
            var routiny = CreateCoroutiny(mono, ie, selfGo);
            routiny.Start();
            return routiny;
        }
    }
}
