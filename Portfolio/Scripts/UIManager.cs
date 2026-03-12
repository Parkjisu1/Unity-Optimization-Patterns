using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class OpenUIBase
{
    private bool isHide;
    private string name;
    private UIBase uiBase;

    public bool IsHide
    {
        get { return isHide; }
        set { isHide = value; }
    }

    public string Name { get { return name; } }
    public UIBase UiBase { get { return uiBase; } }

    public OpenUIBase(string _name, UIBase _uiBase)
    {
        name = _name;
        uiBase = _uiBase;
    }

    public int GetSiblingIndex()
    {
        return uiBase.gameObject.transform.GetSiblingIndex();
    }

    public void SetSiblingIndex(int _index)
    {
        uiBase.gameObject.transform.SetSiblingIndex(_index);
    }
}

public class OpenUIObj
{
    public string name;
    public GameObject gameObject;

    public OpenUIObj(string _name, GameObject _gameObject)
    {
        name = _name;
        gameObject = _gameObject;
    }
}

public class UIManager : Singletone<UIManager>
{
    private List<OpenUIBase> openUIBaseList = new List<OpenUIBase>();
    private List<OpenUIObj> openUIObjectList = new List<OpenUIObj>();

    private Dictionary<int, Dictionary<OpenUIBase, OpenUIObj>> saveOpenUIDic = new Dictionary<int, Dictionary<OpenUIBase, OpenUIObj>>();
    private int saveDicKey = 0;

    [Header("[Canvas Scaler]")]
    public UnityEngine.UI.CanvasScaler CanvasScaler;
    public float UIEffectScale = 1f;
    public float UIZoomInOutScale
    {
        get
        {
            RectTransform _rectTr = (RectTransform)CanvasScaler.transform;
            return _rectTr.sizeDelta.y / 720f;
        }
    }

    public float ScreenRatio
    {
        get
        {
            return (float)Screen.height / (float)Screen.width;
        }
    }
    private float screenWid = 0;
    private float screenHei = 0;

    public float ScreenWid
    {
        get
        {
            if (screenWid == 0)
                InitScreenSize();

            return screenWid;
        }
    }
    public float ScreenHei
    {
        get
        {
            if (screenHei == 0)
                InitScreenSize();
            return screenHei;
        }
    }


    [Header("[Public UI Parent]")]
    public Transform UiTr;

    [Header("[Hud Parent]")]
    public Transform HudTr;

    [Header("[Toast Msg Parent]")]
    public Transform UiToastMsgTr;

    //public UIToastMsg ToastMsg { get; private set; }

    [Header("[Toast UI Parent]")]
    public Transform UiToastTr;
    private GameObject uiToastObj = null;
    private Queue<Dictionary<string, object[]>> chkUiToastLIst = new Queue<Dictionary<string, object[]>>();

    void OnRectTransformDimensionsChange()
    {
        Start();
    }

    void Start()
    {
        if (CanvasScaler == null)
            return;

        float ratio = (float)Screen.height / (float)Screen.width;
        if (ratio <= 0.5625f)
            CanvasScaler.matchWidthOrHeight = 1f;
        else
            CanvasScaler.matchWidthOrHeight = 0;

        if (ratio > 0.5625 && CanvasScaler.matchWidthOrHeight != 1)
        {
            UIEffectScale = (720f / 1280f) / ratio;
        }
    }

    private void LateUpdate()
    {
        ChkUiToastList();
    }

    void InitScreenSize()
    {
        RectTransform _canvasRect = CanvasScaler.GetComponent<RectTransform>();
        screenWid = _canvasRect.sizeDelta.x;
        screenHei = _canvasRect.sizeDelta.y;
    }

    public static Hashtable Hash(params object[] _args)
    {
        Hashtable _hashtable = new Hashtable(_args.Length / 2);
        if (_args.Length % 2 != 0)
        {
            Debug.LogError("UIManager.Hash() Error : Hash requires an even number of arguments!");
            return null;
        }
        else
        {
            int _i = 0;
            while (_i < _args.Length - 1)
            {
                _hashtable.Add(_args[_i], _args[_i + 1]);
                _i += 2;
            }
            return _hashtable;
        }
    }

    #region UI
    OpenUIBase GetOpenUIBase(string _name)
    {
        for (int i = 0; i < openUIBaseList.Count; i++)
        {
            if (openUIBaseList[i].Name == _name)
                return openUIBaseList[i];
        }

        return null;
    }

    public UIBase GetTopOpenUIBase()
    {
        if (openUIBaseList.Count > 0)
            return openUIBaseList[openUIBaseList.Count - 1].UiBase;
        return null;
    }

    public string GetTopOpenUIBaseName()
    {
        if (openUIBaseList.Count > 0)
            return openUIBaseList[openUIBaseList.Count - 1].Name;

        return string.Empty;
    }

    GameObject GetOpenUIObject(string _name)
    {
        for (int i = 0; i < openUIObjectList.Count; i++)
        {
            if (openUIObjectList[i].name == _name)
                return openUIObjectList[i].gameObject;
        }

        return null;
    }

    public int OpenUICount
    {
        get
        {
            return openUIBaseList.Count;
        }
    }

    //public void CreateUILoading()
    //{
    //    GameObject _gameObject = Instantiate(Resources.Load("Prefabs/UI/Loading/UILoading") as GameObject, transform);
    //    _gameObject.transform.localPosition = Vector3.zero;

    //    UILoading _uiLoading = _gameObject.GetComponent<UILoading>();
    //    UILoading.Instance = _uiLoading;
    //    _uiLoading.Show(false);
    //}

    //public void CreateUINetConnect()
    //{
    //    // UINetConntect
    //    GameObject _gameObject = Instantiate(Resources.Load("Prefabs/UI/Loading/UINetConnect") as GameObject, transform);
    //    _gameObject.transform.localPosition = Vector3.zero;

    //    UINetConnect _netConnect = _gameObject.GetComponent<UINetConnect>();
    //    _netConnect.Show(false);
    //}

    //public void CreateToastMsg()
    //{
    //    if (ToastMsg == null)
    //    {
    //        GameObject _gameObject = GameObject.Instantiate(Resources.Load("Prefabs/UI/Toast/UIToastMsg") as GameObject, UiToastMsgTr);
    //        ToastMsg = _gameObject.GetComponent<UIToastMsg>();
    //        ToastMsg.View(false);
    //    }
    //}

    /// <summary>
    /// UIManager.Hash("path", "Prefabs/이하 폴더 경로")
    /// </summary>
    public void OpenUI<T>(Hashtable _hashtable = null)
    {
        GameObject _gameObject = GetOpenUIObject(typeof(T).Name);
      //  Debug.LogError ($"_gameObject : {_gameObject}, _hashtable : {_hashtable}");
        if (_gameObject == null)
        {
            Transform _transform = UiTr;
            if (_hashtable != null && _hashtable.ContainsKey("hud"))
                _transform = HudTr;
            else
                HideCurrentUI();

            string _path = "1_Prefab/";
            if (_hashtable != null && _hashtable.ContainsKey("path"))
                _path = _path + (string)_hashtable["path"] + "/";



            if (_hashtable != null && _hashtable.ContainsKey("parent"))
                _transform = (Transform)_hashtable["parent"];

            _gameObject = Instantiate(Resources.Load(_path + typeof(T).Name) as GameObject, _transform);
        }

        UIBase _uiBase = _gameObject.GetComponent<UIBase>();
        _uiBase.Hashtable = _hashtable;
        _uiBase.name = typeof(T).Name;

        if (_hashtable != null && (_hashtable.ContainsKey("hud") || _hashtable.ContainsKey("chat")))
        {
            _uiBase.OpenUI();
            return;
        }

        if (GetOpenUIBase(_uiBase.name) == null)
            openUIBaseList.Add(new OpenUIBase(_uiBase.name, _uiBase));

        if (GetOpenUIObject(_uiBase.name) == null)
            openUIObjectList.Add(new OpenUIObj(_uiBase.name, _gameObject));

        _uiBase.OpenUI();
    }


    void SetHudView()
    {
    }

    public void RecvOpenUI<T>(Hashtable _hashtable = null)
    {
        StartCoroutine(recvOpenUI<T>(_hashtable));
    }

    IEnumerator recvOpenUI<T>(Hashtable _hashtable = null)
    {
        yield return new WaitForSeconds(0.1f);

        OpenUI<T>(_hashtable);
    }

    public bool IsOpenUI<T>()
    {
        if (GetOpenUIBase(typeof(T).Name) == null)
            return false;
        return true;
    }

    public bool IsActiveUI<T>()
    {
        GameObject _object = GetOpenUIObject(typeof(T).Name);

        if (_object != null && _object.activeSelf)
            return true;

        return false;
    }

    public void OpenUILobby()
    {
        CloseUIAll(new string[] { "UILobby" });

        return;
    }

    public void OnCloseCurrent()
    {
        UIBase _uiBase = CurrentUI();
        if (_uiBase != null)
            _uiBase.OnClickBackFromHud();
    }

    public UIBase CurrentUI()
    {
        if (openUIBaseList.Count > 0)
            return openUIBaseList[openUIBaseList.Count - 1].UiBase;

        return null;
    }

    public void CloseUI<T>()
    {
        CloseUI(typeof(T).Name);
    }

    public void CloseUI(string _uiBaseName)
    {
        for (int i = openUIBaseList.Count - 1; i >= 0; i--)
        {
            if (openUIBaseList[i].Name == _uiBaseName)
            {
                openUIBaseList[i].UiBase.CloseUI();
                openUIBaseList.RemoveAt(i);
                break;
            }
        }

        for (int i = openUIObjectList.Count - 1; i >= 0; i--)
        {
            if (openUIObjectList[i].name == _uiBaseName)
            {
                openUIObjectList.RemoveAt(i);
                break;
            }
        }

        CheckOpenUI();
    }

    public void CloseCurrentUI()
    {
        CloseCurrentUI(null);
    }

    public void CloseCurrentUI(Hashtable _hashtable = null)
    {
        if (openUIBaseList.Count > 0)
        {
            CloseUI(openUIBaseList[openUIBaseList.Count - 1].Name);
        }

        CheckOpenUI(_hashtable);
    }

    void CheckOpenUI(Hashtable _hashtable = null)
    {
        if (openUIBaseList.Count == 1 && openUIBaseList[0].IsHide)
        {
            openUIBaseList[0].IsHide = false;

            openUIBaseList[0].UiBase.Hashtable = _hashtable;
            openUIBaseList[0].UiBase.ReOpenUI();
            openUIBaseList[0].UiBase.ShowUI();
        }

        if (openUIBaseList.Count < 1)
        {
            //JoystickPadShow(true);
            return;
        }

        int _siblingIndex = openUIBaseList[openUIBaseList.Count - 1].GetSiblingIndex() + 1;
        openUIBaseList[openUIBaseList.Count - 1].SetSiblingIndex(_siblingIndex);

        openUIBaseList[openUIBaseList.Count - 1].UiBase.Hashtable = _hashtable;
        openUIBaseList[openUIBaseList.Count - 1].UiBase.ReOpenUI();

    }

    public void CloseUIAll(string[] _notClosebaseName = null)
    {
        if (_notClosebaseName == null)
        {
            for (int i = openUIBaseList.Count - 1; i >= 0; i--)
            {
                openUIBaseList[i].UiBase.CloseUI();
                openUIBaseList.RemoveAt(i);

                DestroyImmediate(openUIObjectList[i].gameObject);
                openUIObjectList.RemoveAt(i);
            }
        }
        else
        {
            bool _close = true;
            for (int i = openUIBaseList.Count - 1; i >= 0; i--)
            {
                _close = true;
                for (int n = _notClosebaseName.Length - 1; n >= 0; n--)
                {
                    if (openUIBaseList[i].Name == _notClosebaseName[n])
                        _close = false;
                }

                if (_close)
                {
                    openUIBaseList[i].UiBase.CloseUI();
                    openUIBaseList.RemoveAt(i);

                    DestroyImmediate(openUIObjectList[i].gameObject);
                    openUIObjectList.RemoveAt(i);
                }
            }
        }

        CheckOpenUI();
    }

    public void HideCurrentUI()
    {
        if (openUIBaseList.Count > 0)
        {
            OpenUIBase _openUIBase = GetOpenUIBase(openUIBaseList[openUIBaseList.Count - 1].Name);

            if (_openUIBase != null && _openUIBase.UiBase != null)
            {
                _openUIBase.IsHide = true;
                _openUIBase.UiBase.HideUI();
            }
        }
    }

    public void HideUI<T>()
    {
        OpenUIBase _openUIBase = GetOpenUIBase(typeof(T).Name);

        if (_openUIBase != null && _openUIBase.UiBase != null)
        {
            _openUIBase.IsHide = true;
            _openUIBase.UiBase.HideUI();
        }
    }

    public void ShowUI<T>()
    {
        OpenUIBase _openUIBase = GetOpenUIBase(typeof(T).Name);

        if (_openUIBase != null && _openUIBase.UiBase != null)
        {
            _openUIBase.IsHide = false;
            _openUIBase.UiBase.ShowUI();
        }
    }

    public void SaveUI()
    {
        if (saveOpenUIDic.ContainsKey(saveDicKey) == false)
            saveOpenUIDic.Add(saveDicKey, new Dictionary<OpenUIBase, OpenUIObj>());

        for (int i = 0; i < openUIBaseList.Count; i++)
        {
            // 현재 IsHIde 값으로 설정
            openUIBaseList[i].IsHide = openUIBaseList[i].IsHide;
            openUIBaseList[i].UiBase.HideUI();

            saveOpenUIDic[saveDicKey].Add(openUIBaseList[i], openUIObjectList[i]);
        }

        openUIBaseList.Clear();
        openUIObjectList.Clear();

        saveDicKey++;

        CheckOpenUI();
    }

    public void LoadUI()
    {
        if (saveOpenUIDic.Count <= 0) return;

        saveDicKey--;

        var _dicEnu = saveOpenUIDic[saveDicKey].GetEnumerator();

        while (_dicEnu.MoveNext())
        {
            openUIBaseList.Add(_dicEnu.Current.Key);
            openUIObjectList.Add(_dicEnu.Current.Value);

            // 저장된 IsHide 값으로 구분해서 끄고 켜줌
            if (_dicEnu.Current.Key.IsHide)
            {
                _dicEnu.Current.Key.IsHide = true;
                _dicEnu.Current.Key.UiBase.HideUI();
            }
            else
            {
                _dicEnu.Current.Key.IsHide = false;
                _dicEnu.Current.Key.UiBase.ShowUI();
            }
        }

        saveOpenUIDic[saveDicKey].Clear();

        CheckOpenUI();
    }
    #endregion

    #region UIToast


    public void OpenUIToast<T>(object[] _data)
    {
        Dictionary<string, object[]> _temp = new Dictionary<string, object[]>();
        _temp.Add(typeof(T).Name, _data);

        chkUiToastLIst.Enqueue(_temp);

        if (uiToastObj != null)
            return;

        ChkUiToastList();
    }

    void ChkUiToastList()
    {
        if (uiToastObj != null || chkUiToastLIst.Count <= 0 )
            return;

        var _dicEnu = chkUiToastLIst.Dequeue().GetEnumerator();
        while (_dicEnu.MoveNext())
        {
            string _path = $"Prefabs/UI/Toast/{_dicEnu.Current.Key}";
            uiToastObj = Instantiate(Resources.Load(_path) as GameObject, UiToastTr);

            //  UIToastBase _uiToastBase = uiToastObj.GetComponent<UIToastBase>();
            //  _uiToastBase.InitData(_dicEnu.Current.Value);
            //  _uiToastBase.Init();

            return;
        }
    }

    public void CloseUIToast()
    {
        if (uiToastObj != null)
            Destroy(uiToastObj);
        uiToastObj = null;
    }

    #endregion

    public GameObject LoadPrefab(string _pathNName, Transform _parent)
    {
        _pathNName = "1_Prefab/" + _pathNName;

        GameObject _gameObject = Instantiate(Resources.Load(_pathNName), _parent) as GameObject;
        _gameObject.transform.localScale = Vector3.one;
        _gameObject.transform.localPosition = Vector3.zero;

        return _gameObject;
    }

    public void LoadPrefabAsset<TObject>(string _pathNName, AssetLoadGameObjectDel<TObject> _rtnDelegate = null, object[] _instantiateData = null)
    {
        try
        {
            string _path = $"1_Prefab/{_pathNName}";
            LoadAsset(_path, _rtnDelegate, _instantiateData);
        }
        catch (Exception e) { };
    }

    public void LoadAsset<TObject>(string _pathName, AssetLoadGameObjectDel<TObject> _rtnDelegate = null, object[] _instantiateData = null)
    {
        Addressables.LoadAssetAsync<TObject>(_pathName).Completed +=
            (AsyncOperationHandle<TObject> _obj) =>
            {
                if (_obj.Status == AsyncOperationStatus.Succeeded)
                {
                    if (_rtnDelegate != null)
                        _rtnDelegate(_obj.Result, _instantiateData);

                    Addressables.Release(_obj);
                }
            };
    }

    public void LoadDefaultUI()
    {
        if (UIHud.Instance == null)
            OpenUI<UIHud>(Hash("path", "UI/Hud", "hud", true));
    }
}



