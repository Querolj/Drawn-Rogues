using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class BloodSplasher : MonoBehaviour
{
    private class BloodSplash : MonoBehaviour
    {
        private SpriteRenderer _rend;
        private float _timeToLive;
        private float _initStartFadeTime;

        public void Init (Sprite sprite, float timeToLive)
        {
            _rend = gameObject.AddComponent<SpriteRenderer> ();
            _rend.sprite = sprite;
            _rend.sortingOrder = 2;
            _timeToLive = timeToLive;
            _initStartFadeTime = timeToLive * 0.25f;
        }

        private void Update ()
        {
            _timeToLive -= Time.deltaTime;
            if (_timeToLive <= _initStartFadeTime)
            {
                _rend.color = new Color (1, 1, 1, _timeToLive / _initStartFadeTime);
            }

            if (_timeToLive <= 0)
            {
                DestroyImmediate (gameObject);
            }
        }
    }

    [SerializeField]
    private Sprite[] _bloodSplashes;

    [SerializeField]
    private AudioClip[] _bloodSound;

    [SerializeField]
    private float _scale = 0.5f;

    [SerializeField]
    private float _splashExtend = 0.1f;

    [SerializeField]
    private float _splashTimeout = 2f;

    private AudioSource _source;

    private void Awake ()
    {
        _source = GetComponent<AudioSource> ();
    }

    public void Splash (Vector3 position, int splashCount = 3)
    {
        int soundIndex = Random.Range (0, _bloodSound.Length);
        _source.clip = _bloodSound[soundIndex];
        _source.Play ();

        for (int i = 0; i < splashCount; i++)
        {
            int splashIndex = Random.Range (0, _bloodSplashes.Length);
            Vector3 randomOffset = new Vector3 (0f, Random.Range (-_splashExtend, _splashExtend), 0f);
            GameObject splash = new GameObject ("splash" + i);
            splash.transform.position = position + randomOffset;
            splash.transform.localScale = new Vector3 (_scale, _scale, _scale);
            BloodSplash bloodsplash = splash.AddComponent<BloodSplash> ();
            bloodsplash.Init (_bloodSplashes[splashIndex], _splashTimeout);
        }
    }
}