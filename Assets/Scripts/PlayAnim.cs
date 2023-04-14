using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayAnim : MonoBehaviour
{
    private List<Sprite> _sprites;
    public List<Texture2D> _texture2Ds;

    public float _time = 0.2f;
    private int _index = -1;
    private float _playerTime;
    private Image _image;
    public bool _autoPlay = true;
    public int _maxFrame = -1;

    public List<Texture2D> texture2Ds 
    {
        get 
        {
            return _texture2Ds;
        } 
        set 
        {
            if (value == null)
                return;

            Dispose();
            _texture2Ds = value;
            _sprites = new List<Sprite>();
            foreach (Texture2D texture2D in value)
            {
                Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                _sprites.Add(sp);
            }
        } 
    }

    private void Start()
    {
        _playerTime = _time;
        _image = GetComponent<Image>();
        texture2Ds = _texture2Ds;
    }


    private void Update()
    {
        if (!_autoPlay)
            return;

        if (_sprites == null)
            return;

        if (_image == null)
            return;

        _maxFrame = _sprites.Count;
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

            _image.sprite = _sprites[_index];
            _image.SetNativeSize();
        }
    }

    public void Dispose()
    {
        _index = 0;
        if (_sprites != null)
        {
            foreach (Sprite sp in _sprites)
            {
                Destroy(sp.texture);
            }

            _sprites.Clear();
            _image.sprite = null;

            LoadTexture.Dispose(_texture2Ds);
            _texture2Ds.Clear();

        }
    }
}
