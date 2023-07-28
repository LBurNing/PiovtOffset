using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateFrameUI : MonoBehaviour
{
    private InputField _startPos;
    private InputField _count;
    private InputField _skip;
    private InputField _maxCount;
    private InputField _name;
    private InputField _dir;
    private Dropdown _dropdown;
    private Button _delete;
    private MirAction _mirAction = MirAction.Node;


    void Start()
    {
        _startPos = transform.Find("StartPos").GetComponent<InputField>();
        _count = transform.Find("Count").GetComponent<InputField>();
        _skip = transform.Find("Skip").GetComponent<InputField>();
        _maxCount = transform.Find("MaxCount").GetComponent<InputField>();
        _name = transform.Find("Name").GetComponent<InputField>();
        _dir = transform.Find("Dir").GetComponent<InputField>();
        _dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        _delete = transform.Find("Delete").GetComponent<Button>();

        InitDropdown();

        _dropdown.onValueChanged.AddListener(OnSelectAction);
        _delete.onClick.AddListener(OnDelete);
    }

    private void OnDelete()
    {
        Destroy(gameObject);
    }

    private void OnSelectAction(int type)
    {
        _mirAction = (MirAction)type;
    }

    private void InitDropdown()
    {
        _dropdown.ClearOptions();
        foreach (MirAction item in Enum.GetValues(typeof(MirAction)))
        {
            Dropdown.OptionData optionData = new Dropdown.OptionData();
            optionData.text = Enum.GetName(typeof(MirAction), item);
            _dropdown.options.Add(optionData);
        }

        _dropdown.value = 0;
        _dropdown.RefreshShownValue();
    }
    
    public MirAction mirAction
    { 
        get 
        { 
            return _mirAction; 
        } 
    }

    public string Name
    {
        get
        {
            return _name.text;
        }
        set
        {
            _name.text = value;
        }
    }

    public Frame Frame
    {
        get
        {
            int start = 0;
            int count = 0;
            int skip = 0;
            int maxCount = 0;
            int dir = 0;
            string Name = "δ֪";

            if(!string.IsNullOrEmpty(_startPos.text))
                start = int.Parse(_startPos.text);

            if (!string.IsNullOrEmpty(_count.text))
                count = int.Parse(_count.text);

            if (!string.IsNullOrEmpty(_skip.text))
                skip = int.Parse(_skip.text);

            if (!string.IsNullOrEmpty(_maxCount.text))
                maxCount = int.Parse(_maxCount.text);

            if (!string.IsNullOrEmpty(_name.text))
                Name = _name.text;

            if (!string.IsNullOrEmpty(_dir.text))
            {
                dir = int.Parse(_dir.text);
                dir = dir == 0 ? 1 : dir;
            }

            Frame frame = new Frame(start, count, skip, maxCount);
            frame.Dir = dir;
            frame.Name = Name;
            return frame;
        }
    }
}
