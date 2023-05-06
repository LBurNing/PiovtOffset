using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayAnim : MonoBehaviour
{
    private List<Sprite> _sprites;
    private List<string> _paths;
    public List<Texture2D> _texture2Ds;

    public float _time = 0.2f;
    private int _index = -1;
    private float _playerTime;
    private Image _image;
    public bool _play = true;
    public int _maxFrame = -1;
    private bool _initTexture2d = false;
    private Action<Vector2> _textureMaxSize;
    private Vector2 _maxSize;

    public Action<Vector2> textureMaxSize
    {
        set { _textureMaxSize = value; }
    }

    public bool IsDispose
    {
        get 
        { 
            return !_initTexture2d && _texture2Ds.Count > 0;
        }
    }

    public List<string> Paths
    {
        set 
        {
            _paths = value;
        }
    }

    public int frameCount
    {
        get
        {
            if (_paths == null) return 0;
            return _paths.Count;
        }
    }

    public void SetTexture2D(Texture2D texture2D, bool dispose = true)
    {
        if (!_initTexture2d)
            _texture2Ds.Add(texture2D);

        Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        _sprites.Add(sp);
    }

    private void Start()
    {
        _playerTime = _time;
        _image = GetComponent<Image>();
        _sprites = new List<Sprite>();
        _paths = new List<string>();
        if (_texture2Ds.Count > 0)
            _initTexture2d = true;

        foreach (var value in _texture2Ds)
        {
            SetTexture2D(value, false);
        }
    }


    private void Update()
    {
        if (!_play)
            return;

        if (_image == null)
            return;

        _maxFrame = _paths.Count;
        if (_maxFrame == 0)
            return;

        _playerTime -= Time.deltaTime;
        if (_playerTime <= 0)
        {
            _playerTime = _time;
            _index++;

            if (_index == _maxFrame)
            {
                _index = 0;
            }

            if (_sprites.Count == _index)
            {
                Texture2D texture2D = LoadTexture.LoadTex(_paths[_index]);
                SetTexture2D(texture2D);
            }

            _image.sprite = _sprites[_index];
            _image.SetNativeSize();

            if (_textureMaxSize != null)
            {
                Vector2Int size = TextureHelper.CalculateBoundingBox(_image.sprite.texture);
                if (size.x > _maxSize.x)
                    _maxSize.x = size.x;

                if (size.y > _maxSize.y)
                    _maxSize.y = size.y;

                _textureMaxSize.Invoke(_maxSize);
                _textureMaxSize = null;
            }
        }
    }

    public bool horMirror
    {
        get
        {
            return transform.localScale.x < 0;
        }
    }

    public bool verMirror
    {
        get
        {
            return transform.localScale.y < 0;
        }
    }

    public void Mirror(bool horizontal)
    {
        float x = transform.localScale.x;
        float y = transform.localScale.y;

        if (horizontal)
        {
            x = transform.localScale.x < 0 ? Mathf.Abs(transform.localScale.x) : -transform.localScale.x;
        }
        else
        {
            y = transform.localScale.y < 0 ? Mathf.Abs(transform.localScale.y) : -transform.localScale.y;
        }


        transform.localScale = new Vector3(x, y, transform.localScale.z);
    }

    public void Dispose()
    {
        if (_sprites != null)
        {
            if (!_initTexture2d)
            {
                foreach (Sprite sp in _sprites)
                {
                    Destroy(sp.texture);
                }

                LoadTexture.Dispose(_texture2Ds);
            }

            _sprites.Clear();
            _texture2Ds.Clear();
            _paths.Clear();
        }

        _index = -1;
        _textureMaxSize = null;
        _maxSize = Vector2.zero;
    }
}
