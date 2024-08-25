using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = Gemity.Api.Debug;

namespace Gemity.Coroutiny
{
    public sealed class Coroutiny : IDisposable
    {
        internal enum State
        {
            Ready = 1,
            Update = 2,
            Pause = 4,
            Completed = 8,
        }

        internal static HashSet<Coroutiny> _allCoroutiny = new();

        private string _name;
        private int _id;
        private IEnumerator _ie;
        private CustomYieldInstruction _yieldInstruction;
        private GameObject _go;
        private bool _autoDestroy;

        internal State _state;

        public event Action<Coroutiny> onStart;
        public event Action<Coroutiny> onUpdate;
        public event Action<Coroutiny> onPause;
        public event Action<Coroutiny> onResume;
        public event Action<Coroutiny> onComplete;
        public event Action<Coroutiny> onDestroy;

        public string Name => _name;
        public int Id => _id;

        static Coroutiny()
        {
            new GameObject("CoroutinyExcuter", typeof(CoroutinyExcuter));
        }
        internal static Coroutiny Create(IEnumerator ie, MonoBehaviour go, bool autoDestroy = false)
        {
            Coroutiny coroutiny = new();
            Guid guid = Guid.NewGuid();
            coroutiny._name = guid.ToString();
            coroutiny._id = guid.GetHashCode();
            coroutiny._ie = ie;
            coroutiny._state = State.Ready;
            coroutiny._go = go.gameObject;
            coroutiny._autoDestroy = autoDestroy;
            go.destroyCancellationToken.Register(() => coroutiny.Destroy());

            return coroutiny;
        }

        public static Coroutiny FindCoroutinyById(int id)
        {
            return _allCoroutiny.FirstOrDefault(x => x.Id == id);
        }

        private Coroutiny() { }

        public void Start()
        {
            _state = State.Update;
            onStart?.Invoke(this);
        }

        public void Pause()
        {
            if (_state != State.Update)
            {
                Debug.LogWarning($"Can't pause coroutiny. State {_state}");
                return;
            }
            _state = State.Pause;
            onPause?.Invoke(this);
        }

        public void Resume()
        {
            _state = State.Update;
            onResume?.Invoke(this);
        }

        public void Destroy()
        {
            onDestroy?.Invoke(this);
            _allCoroutiny.Remove(this);
            Dispose();
        }

        public Coroutiny Reset()
        {
            _yieldInstruction = null;
            _ie.Reset();
            _state = State.Ready;

            return this;
        }

        public Coroutiny SetName(string name)
        {
            _name = name;
            return this;
        }

        public Coroutiny SetId(int id)
        {
            _id = id;
            return this;
        }

        public Coroutiny SetAutoDestroy(bool autoDestroy)
        {
            _autoDestroy = autoDestroy;
            return this;
        }

        internal void Update()
        {
            if (_state != State.Update || !_go.activeInHierarchy)
                return;

            onUpdate?.Invoke(this);

            if (_yieldInstruction != null)
            {
                if (_yieldInstruction.keepWaiting)
                    return;
                else
                    _yieldInstruction = null;
            }

            if (_ie.MoveNext())
            {
                if (_ie.Current == null)
                    return;

                switch (_ie.Current)
                {
                    case CustomYieldInstruction customYieldInstruction:
                        _yieldInstruction = customYieldInstruction;
                        break;
                    case WaitForSeconds waitForSeconds:
                        {
                            FieldInfo info = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
                            _yieldInstruction = new WaitForSecondsTiny((float)info.GetValue(waitForSeconds));
                            break;
                        }

                    default:
                        Debug.LogError($"YieldInstruction Type {_ie.Current.GetType()} not support");
                        break;
                }
            }
            else
            {
                _state = State.Completed;
                onComplete?.Invoke(this);

                if(_autoDestroy)
                    Destroy();
            }
        }

        public void Dispose()
        {
            onStart = null;
            onUpdate = null;
            onPause = null;
            onResume = null;
            onComplete = null;
            onDestroy = null;
        }
    }
}
