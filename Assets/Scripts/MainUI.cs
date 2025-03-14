using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MainUI : MonoBehaviour
{
    #region Button
    private Button exportBtn;
    private Button playTempleteBtn;
    private Button playModifyBtn;
    private Button refactorBtn;
    private Button expandAnimation;
    private Button horizontal;
    private Button vertical;
    private Button alpha;
    private Button bmp2Png;
    private Button merge;
    private Button pList2Png;
    #endregion

    #region Toggle
    private Toggle playFrame;
    private Toggle IrregularToggle;
    private Toggle mapToggle;
    private Toggle isScale;
    private Toggle lockFrame;
    #endregion

    #region InputField
    private InputField offsetX;
    private InputField offsetY;
    private InputField templetePath;
    private InputField modifyPath;
    private InputField scaleWidth;
    private InputField scaleHeight;
    #endregion

    #region Text
    private Text progressText;
    #endregion

    #region Script
    private PlayAnim templeteAnim;
    private PlayAnim modifyAnim;
    private FrameUI templeteFrameUI;
    private IrregularUI irregularUI;
    private MapSettingsUI mapSettingsUI;
    private List<FrameUI> frameUIs;
    private UpdateBtnText boxCom;
    #endregion

    #region Image
    private Image progressImage;
    #endregion

    #region RectTransform
    private RectTransform sprites;
    private RectTransform frames;
    private RectTransform itemBg;
    private RectTransform box;
    #endregion

    #region Scrollbar
    private Scrollbar scaleScrollbar;
    private Scrollbar rotationScrollbar;
    private Scrollbar cutScrollbar;
    #endregion

    #region Dropdown
    private Dropdown dropdown;
    private Dropdown dirDropdown;
    #endregion

    private Vector3 boxDefaultScale;
    private Action<float> progressCallBack;
    private Coroutine exportCoroutine;

    private float playTime = 0.2f;
    private float speed = 0.2f;
    private int playIndex = -1;
    private int selectIndex = -1;
    private int templeteAnimIndex = 0;
    private int modifyAnimIndex = 0;
    private float defaultScale = 1;
    private float curScale = 1;
    private bool refactor = true;
    private int modifyLayer = 0;
    private int templeteLayer = 0;

    private EnumResType exportType = EnumResType.NeiGuan;
    private EnumDirection animDir = EnumDirection.None;

    void Start()
    {
        frameUIs = new List<FrameUI>();

        #region Button
        exportBtn = transform.Find("ExportBtn").GetComponent<Button>();
        playTempleteBtn = transform.Find("Select/PlayTemplete").GetComponent<Button>();
        playModifyBtn = transform.Find("Select/PlayModify").GetComponent<Button>();
        expandAnimation = transform.Find("Select/ExpandAnimation").GetComponent<Button>();
        refactorBtn = transform.Find("RefactorBtn").GetComponent<Button>();
        horizontal = transform.Find("Select/Horizontal").GetComponent<Button>();
        alpha = transform.Find("Select/Alpha").GetComponent<Button>();
        bmp2Png = transform.Find("Select/Bmp2Png").GetComponent<Button>();
        vertical = transform.Find("Select/Vertical").GetComponent<Button>();
        merge = transform.Find("Select/Merge").GetComponent<Button>();
        pList2Png = transform.Find("Select/pList2Png").GetComponent<Button>();
        #endregion

        #region Toggle
        playFrame = transform.Find("PlayFrame").GetComponent<Toggle>();
        IrregularToggle = transform.Find("IrregularToggle").GetComponent<Toggle>();
        mapToggle = transform.Find("mapToggle").GetComponent<Toggle>();
        isScale = transform.Find("Select/IsScale").GetComponent<Toggle>();
        lockFrame = transform.Find("Select/LockFrame").GetComponent<Toggle>();
        #endregion

        #region InputField
        scaleWidth = transform.Find("Select/ScaleWidth").GetComponent<InputField>();
        scaleHeight = transform.Find("Select/ScaleHeight").GetComponent<InputField>();
        templetePath = transform.Find("Select/TempletePath").GetComponent<InputField>();
        modifyPath = transform.Find("Select/ModifyPath").GetComponent<InputField>();
        #endregion

        #region Scrollbar
        scaleScrollbar = transform.Find("Select/ScaleScrollbar").GetComponent<Scrollbar>();
        rotationScrollbar = transform.Find("Select/RotationScrollbar").GetComponent<Scrollbar>();
        cutScrollbar = transform.Find("Select/CutScrollbar").GetComponent<Scrollbar>();
        #endregion

        #region RectTransform
        sprites = transform.Find("Sprites").GetComponent<RectTransform>();
        frames = transform.Find("Frames").GetComponent<RectTransform>();
        itemBg = transform.Find("ItemBg").GetComponent<RectTransform>();
        box = transform.Find("Box").GetComponent<RectTransform>();
        #endregion

        #region Script
        templeteAnim = transform.Find("TempleteAnim").GetComponent<PlayAnim>();
        modifyAnim = transform.Find("ModifyAnim").GetComponent<PlayAnim>();
        templeteFrameUI = transform.Find("Templete").GetComponent<FrameUI>();
        irregularUI = transform.Find("Irregular").GetComponent<IrregularUI>();
        mapSettingsUI = transform.Find("mapSettings").GetComponent<MapSettingsUI>();
        boxCom = box.GetComponent<UpdateBtnText>();
        #endregion

        #region Text
        progressText = transform.Find("ProgressBg/ProgressText").GetComponent<Text>();
        #endregion

        #region Image
        progressImage = transform.Find("ProgressBg/Progress").GetComponent<Image>();
        #endregion

        #region DropDown
        dropdown = transform.Find("RefactorType/Dropdown").GetComponent<Dropdown>();
        dirDropdown = transform.Find("Select/DirDropdown").GetComponent<Dropdown>();
        #endregion

        #region 默认值
        templeteAnimIndex = templeteAnim.transform.GetSiblingIndex();
        modifyAnimIndex = modifyAnim.transform.GetSiblingIndex();
        dirDropdown.value = (int)EnumDirection.None;
        scaleWidth.text = itemBg.sizeDelta.x.ToString();
        scaleHeight.text = itemBg.sizeDelta.y.ToString();
        irregularUI.MainUI = this;
        mapSettingsUI.MainUI = this;
        modifyLayer = modifyAnim.layer;
        templeteLayer = templeteAnim.layer;
        #endregion

        AddEvent();
    }

    private void AddEvent()
    {
        dirDropdown.onValueChanged.AddListener(OnDirClick);
        exportBtn.onClick.AddListener(OnExportClick);
        playTempleteBtn.onClick.AddListener(OnPlayTempleteBtn);
        playModifyBtn.onClick.AddListener(OnPlayModifyBtn);
        refactorBtn.onClick.AddListener(OnRefactorBtn);
        expandAnimation.onClick.AddListener(OnFrameSpriteBtn);
        playFrame.onValueChanged.AddListener(OnFrameValueChanged);
        dropdown.onValueChanged.AddListener(OnExportTypeClick);
        IrregularToggle.onValueChanged.AddListener(OnIrregularValueChanged);
        mapToggle.onValueChanged.AddListener(OnMapValueChanged);
        scaleWidth.onValueChanged.AddListener(OnWidthValueChanged);
        scaleHeight.onValueChanged.AddListener(OnHeightValueChanged);
        isScale.onValueChanged.AddListener(OnIsScaleValueChanged);
        scaleScrollbar.onValueChanged.AddListener(OnScaleScrollbar);
        rotationScrollbar.onValueChanged.AddListener(onRotationScrollbar);
        cutScrollbar.onValueChanged.AddListener(onCutScrollbar);
        horizontal.onClick.AddListener(OnHorizontal);
        vertical.onClick.AddListener(OnVertical);
        alpha.onClick.AddListener(OnAlpha);
        bmp2Png.onClick.AddListener(OnBmp2Png);
        merge.onClick.AddListener(OnMerge);
        pList2Png.onClick.AddListener(OnpList2Png);
        lockFrame.onValueChanged.AddListener(OnLockFrameChange);
    }

    private void OnBmp2Png()
    {
        ImageTools.Bmp2Png(modifyPath.text);
    }

    private void OnAlpha()
    {
        ImageTools.BlendTexture(modifyPath.text);
    }

    private void OnpList2Png()
    {
        ImageTools.pList2Png(modifyPath.text);
    }

    private void OnMerge()
    {
        StartCoroutine(Merge(ShowProgress));
    }

    private IEnumerator Merge(Action<int, int, string> progressCallBack)
    {
        string path = modifyPath.text;
        int progress = 0;
        string[] filePaths = Directory.GetFiles(path);
        int total = filePaths.Length;

        while (progress < total)
        {
            string filePath = filePaths[progress];
            ImageTools.Merge(filePath);

            progress++;
            progressCallBack?.Invoke(progress, total, "");
            yield return new WaitForSeconds(ExportRes.frameTime);
        }

        yield return new WaitForSeconds(0.5f);
        PlayModifyAnim(modifyPath.text);
    }

    private void OnHorizontal()
    {
        modifyAnim?.Mirror(true);
    }

    private void OnVertical()
    {
        modifyAnim?.Mirror(false);
    }

    private void onCutScrollbar(float value)
    {
        if (boxDefaultScale == Vector3.zero)
            return;

        Vector3 scale = boxDefaultScale * value;
        UpdateBoxScale(scale);
    }

    private void onRotationScrollbar(float value)
    {
        if (isScale.isOn)
        {
            int rotation = (int)(360 - value * 360);
            SetRotation(rotation);
        }
    }

    private void OnScaleScrollbar(float value)
    {
        if (isScale.isOn)
        {
            SetScale((value * 3) - defaultScale + 1);
        }
    }

    private void OnIsScaleValueChanged(bool isOn)
    {
        itemBg.gameObject.SetActive(isOn);
        box.gameObject.SetActive(isOn);

        if (string.IsNullOrEmpty(templetePath.text))
        {
            templeteAnim.gameObject.SetActive(!isOn);
        }
        else
        {
            modifyAnim.transform.SetSiblingIndex(isOn ? templeteAnimIndex : modifyAnimIndex);
        }

        if (!isOn)
        {
            SetScale(1);
            SetRotation(0);
            rotationScrollbar.value = 0;
            scaleScrollbar.value = 0.3333f;
        }

        rotationScrollbar?.gameObject.SetActive(isOn);
        scaleScrollbar?.gameObject.SetActive(isOn);
        cutScrollbar?.gameObject.SetActive(isOn);
        templeteAnim.transform.Find("Line").gameObject.SetActive(!isOn);
    }

    private void OnWidthValueChanged(string width)
    {
        if (string.IsNullOrEmpty(width))
            return;

        itemBg.sizeDelta = new Vector2(float.Parse(width), itemBg.sizeDelta.y);
    }

    private void OnHeightValueChanged(string height)
    {
        if (string.IsNullOrEmpty(height))
            return;

        itemBg.sizeDelta = new Vector2(itemBg.sizeDelta.x, float.Parse(height));
    }

    public string ModifyPath
    {
        get
        {
            return modifyPath.text;
        }
    }

    void Update()
    {
        UpdateInput();
        PlayerFrame();
    }

    private void UpdateInput()
    {
        RectTransform rectTransform = modifyAnim.GetComponent<RectTransform>();
        if (selectIndex >= 0)
            rectTransform = sprites.GetChild(selectIndex).GetComponent<RectTransform>();

        int offsetX = 0;
        int offsetY = 0;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            offsetX = -1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            offsetX = 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            offsetY = 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            offsetY = -1;
        }

        rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);
    }

    private void PlayerFrame()
    {
        if (!playFrame.isOn)
            return;

        if (frameUIs == null || frameUIs.Count == 0)
            return;

        playTime -= Time.deltaTime;
        if (playTime <= 0)
        {
            playTime = speed;
            playIndex++;

            if (playIndex == frameUIs.Count)
            {
                playIndex = 0;
            }

            FrameUI frame = frameUIs[playIndex];
            frame.isOn = false;
            frame.isOn = true;
        }
    }

    private void OnExportTypeClick(int type)
    {
        exportType = (EnumResType)type;
    }

    private void OnIrregularValueChanged(bool isOn)
    {
        irregularUI?.gameObject.SetActive(isOn);
    }

    private void OnMapValueChanged(bool isOn)
    {
        mapSettingsUI?.gameObject.SetActive(isOn);
    }

    private void OnFrameValueChanged(bool isOn)
    {
        foreach (FrameUI frame in frameUIs)
        {
            if (isOn)
                frame.group = frame.transform.parent.GetComponent<ToggleGroup>();
            else
                frame.group = null;
        }
    }

    private void OnDirClick(int value)
    {
        animDir = (EnumDirection)value;
        if (animDir == EnumDirection.UpRight || animDir == EnumDirection.Right || animDir == EnumDirection.DownRight)
        {
            modifyAnim.layer = templeteLayer;
            templeteAnim.layer = modifyLayer;
        }
        else
        {
            modifyAnim.layer = modifyLayer;
            templeteAnim.layer = templeteLayer;
        }

        if (animDir != EnumDirection.None)
        {
            PlayModifyAnim(modifyPath.text);
            PlayTempleteAnim(templetePath.text);
        }
    }

    private void OnLockFrameChange(bool isOn)
    {
        PlayModifyAnim(modifyPath.text);
        PlayTempleteAnim(templetePath.text);
    }

    private void OnFrameSpriteBtn()
    {
        List<string> filePaths = Utils.GetAllFileList(modifyPath.text, ".png");
        if (filePaths.Count > 50)
            return;

        DisposeFrameSprite();
        List<Texture2D> texture2Ds = LoadAssetHelper.Load(modifyPath.text);
        for (int i = 0; i < texture2Ds.Count; i++)
        {
            GameObject frameSpriteGo = new GameObject(i.ToString());
            frameSpriteGo.transform.SetParent(sprites.transform, false);
            RectTransform tf = frameSpriteGo.AddComponent<RectTransform>();
            tf.anchoredPosition = Vector2.zero;
            frameSpriteGo.AddComponent<Image>();
            frameSpriteGo.AddComponent<DragHandle>();
            frameSpriteGo.SetActive(false);

            Image image = tf.GetComponent<Image>();
            Texture2D texture2D = texture2Ds[i];
            image.sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            image.SetNativeSize();

            GameObject frameGo = Instantiate(templeteFrameUI.gameObject);
            frameGo.name = i.ToString();
            frameGo.transform.SetParent(frames.transform, false);
            FrameUI frame = frameGo.GetComponent<FrameUI>();
            frame.index = i;
            frame.frameSprite = tf;
            frame.callback = FrameValueChangedCallBack;
            frameGo.SetActive(true);
            frameUIs.Add(frame);
        }
    }

    private void FrameValueChangedCallBack(int index)
    {
        selectIndex = index;
    }

    private void OnRefactorBtn()
    {
        if (refactor)
        {
            ExportRes.Fill(modifyPath.text);
            ExportRes.Rename(modifyPath.text);
            ImageTools.TxtFormat(modifyPath.text);
            if (exportType == EnumResType.NeiGuan)
            {
                exportCoroutine = StartCoroutine(ExportRes.Export(modifyPath.text, Vector2Int.zero, 10000 * (int)EnumDirection.Down, ShowProgress));
                modifyPath.text = modifyPath.text + @"\待机\";
            }
            else
            {
                ResData resData = new ResData(modifyPath.text);
                exportCoroutine = StartCoroutine(ExportRes.ExportType(exportType, resData, ShowProgress));
            }
        }
        else
        {
            if (exportCoroutine != null)
            {
                StopCoroutine(exportCoroutine);
            }
            
            ShowProgress(0, 0);
        }

        refactor = !refactor;
        refactorBtn.GetComponent<UpdateBtnText>().text = refactor ? "重构" : "暂停";
    }

    public void ShowProgress(int cur, int total, string text = "")
    {
        if (progressText == null)
            return;

        progressText.text = string.Format("{0}{1}/{2}", text, cur, total);
        float x = (float)cur * (float)590.0f / (float)total;
        progressImage.rectTransform.sizeDelta = new Vector2(x, progressImage.rectTransform.sizeDelta.y);
    }

    private void OnPlayTempleteBtn()
    {
        PlayTempleteAnim(templetePath.text);
    }

    private void OnPlayModifyBtn()
    {
        dirDropdown.value = (int)EnumDirection.None;
        PlayModifyAnim(modifyPath.text);
    }

    private void PlayTempleteAnim(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        int dir = (int)EnumDirection.Down + 1;
        if (animDir != EnumDirection.None)
            dir = (int)animDir + 1;

        int maxCount = lockFrame.isOn ? 1 : -1;
        List<string> filePaths = Utils.GetAllFileList(path, ".png", (dir * 100).ToString(), maxCount);
        if (filePaths.Count == 0)
        {
            filePaths = Utils.GetAllFileList(path, ".png");
            Notice.ShowNotice(string.Format("没有找到{0}方向上的资源, 默认播放文件夹下的所有图片", dir.ToString()), Color.red, 3);
        }

        templeteAnim.Dispose();
        templeteAnim.Paths = filePaths;
    }

    private void PlayModifyAnim(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        int dir = (int)EnumDirection.Down + 1;
        if (animDir != EnumDirection.None)
            dir = (int)animDir + 1;

        int maxCount = lockFrame.isOn ? 1 : -1;
        List<string> filePaths = Utils.GetAllFileList(path, ".png", (dir * 100).ToString(), maxCount);
        if (modifyAnim.IsDispose)
            modifyAnim.Dispose();

        if (filePaths.Count == 0)
        {
            filePaths = Utils.GetAllFileList(path, ".png");
            Notice.ShowNotice(string.Format("没有找到{0}方向上的资源, 默认播放文件夹下的所有图片", dir.ToString()), Color.red, 3);
        }

        modifyAnim.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        modifyAnim.Paths = filePaths;
        modifyAnim.textureMaxSize = ImageMaxSize;
    }

    private void ImageMaxSize(Vector2 size)
    {
        cutScrollbar.value = 1;
        box.gameObject.SetActive(isScale.isOn);
        UpdateBoxScale(new Vector3(size.x / 100.0f, size.y / 100.0f, 0));
        boxDefaultScale = boxCom.scale;
    }

    private void UpdateBoxScale(Vector3 scale)
    {
        int width = (int)(scale.x * 100);
        int height = (int)(scale.y * 100);
        int size = TextureHelper.GetAtlasSize(width, height, modifyAnim.frameCount);
        boxCom.scale = new Vector3(scale.x, scale.y, 0);
        boxCom.text = string.Format("裁切 w={0} h={1} c={2} a=<color=#ff0000>{3}</color>", width, height, modifyAnim.frameCount, size);
    }

    private void OnExportClick()
    {
        StartCoroutine(Export(ShowProgress));
    }

    private IEnumerator Export(Action<int, int, string> progressCallBack)
    {
        //缩放+偏移
        string path = modifyPath.text;
        if (curScale != 1 || modifyAnimaOffset != Vector2Int.zero)
            ImageTools.ScaleTexture(path, curScale, modifyAnimaOffset, (int)animDir + 1);

        //裁切
        if (isScale.isOn && cutScrollbar.value != 1)
        {
            int width = (int)(boxCom.scale.x * 100);
            int height = (int)(boxCom.scale.y * 100);
            ImageTools.CutTexture(path, width, height);
        }

        if (frameUIs.Count > 0)
        {
            //不规则偏移
            int progress = 0;
            string[] filePaths = Directory.GetFiles(path);
            int total = filePaths.Length;

            while (progress < total)
            {
                if (frameUIs.Count == filePaths.Length)
                    modifyAnimaOffset = frameUIs[progress].offset;

                string filePath = filePaths[progress];
                ImageTools.Load(filePath, modifyAnimaOffset);

                progress++;
                progressCallBack?.Invoke(progress, total, "");
                yield return new WaitForSeconds(ExportRes.frameTime);
            }
        }

        yield return new WaitForSeconds(0.5f);
        ExportComplete();
    }

    private void DisposeFrameSprite()
    {
        modifyAnim.Dispose();
        frameUIs.Clear();
        modifyAnimaOffset = Vector2Int.zero;
        modifyAnim.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < sprites.childCount; i++)
        {
            Destroy(sprites.GetChild(i).gameObject);
            Destroy(frames.GetChild(i).gameObject);
        }
    }

    private void ExportComplete()
    {
        if (isScale.isOn && (modifyAnimaOffset == Vector2Int.zero))
        {
            ImageTools.Rotate(modifyPath.text, modifyAnim.transform.localRotation.eulerAngles.z);
            SetRotation(0);
            rotationScrollbar.value = 0;
        }

        if (modifyAnim.horMirror)
        {
            ImageTools.Mirror(EmMirror.hor, modifyPath.text);
        }

        if (modifyAnim.verMirror)
        {
            ImageTools.Mirror(EmMirror.ver, modifyPath.text);
        }

        SetScale(1);
        selectIndex = -1;
        scaleScrollbar.value = 0.3333f;

        if (boxDefaultScale != Vector3.zero)
        {
            cutScrollbar.value = 1;
            UpdateBoxScale(boxDefaultScale);
        }

        DisposeFrameSprite();
        PlayModifyAnim(modifyPath.text);
    }

    private void SetScale(float scale)
    {
        scale = scale < 0 ? 0 : scale;
        curScale = scale;
        modifyAnim.transform.localScale = curScale * Vector2.one;
        scaleScrollbar.GetComponent<UpdateBtnText>().text = curScale.ToString();
    }

    private void SetRotation(int rotation)
    {
        if (rotation == 0)
            modifyAnim.transform.localRotation = Quaternion.identity;

        modifyAnim.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        rotationScrollbar.GetComponent<UpdateBtnText>().text = (360 - rotation).ToString();
    }

    private Vector2Int modifyAnimaOffset 
    {
        get
        {
            Vector2Int offset = Vector2Int.zero;
            offset.x = Mathf.RoundToInt(modifyAnim.GetComponent<RectTransform>().anchoredPosition.x);
            offset.y = Mathf.RoundToInt(modifyAnim.GetComponent<RectTransform>().anchoredPosition.y);
            return offset;
        }

        set
        {
            modifyAnim.GetComponent<RectTransform>().anchoredPosition = value;
        }
    }
}
