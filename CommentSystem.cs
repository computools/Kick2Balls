using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Message
{
    public int IdMsg;
    public string Msg;
    public string Action;
    public int CountKick;
    public int CountFive;
    public string countryCode;
    public string MsgDate;
}

public class CommentSystem : MonoBehaviour {
    protected string url = "https://kick2balls.com/src/special_link.php";

    public InputField dateInputField;
    public Button GetDate;
    public Text NoCommentText;
    public Button closeComment;
    public GameObject CommentWindow;
    public List<GameObject> colliderWhoHave;
    public GameObject Bg;

    public List<Message> ListMessage = new List<Message>();

    public GameObject listPrefab;
    public GameObject parentObject;

    public GameObject cellObj;

    public List<GameObject> cells;

    public string iduser = "0";
    public int idserveruser = 0;
    
    // Use this for initialization
    void Start () 
    {
        Invoke("secondStart", 1.5f);
    }

    public void secondStart()
    {
        iduser = this.GetComponent<USA>().serverId;
        int.TryParse(iduser, out idserveruser);
        GetDate.onClick.AddListener(delegate { GetCommentByDate(); });
        closeComment.onClick.AddListener(delegate { closeCommentHandler(); });
    }

    public void kickButtonHandler(int index, int messageID)
    {
        StartCoroutine(MsgActionAsync(idserveruser, messageID, "Kick2Balls"));
        UpdateList();
        StartCoroutine(NotificationAsync(idserveruser));
    }

    public void highButtonHandler(int index, int messageID)
    {
        StartCoroutine(MsgActionAsync(idserveruser, messageID, "HighFive"));
        UpdateList();
        StartCoroutine(NotificationAsync(idserveruser));
    }

    public void reportButtonHandler(int index, int messageID)
    {
        StartCoroutine(ReportAsync(idserveruser, messageID));
        UpdateList();
        StartCoroutine(NotificationAsync(idserveruser));
    }

    public void closeCommentHandler()
    {
        DestroyList();
        CommentWindow.SetActive(false);
        Bg.SetActive(false);

        for (int i = 0; i < colliderWhoHave.Count; i++)
        {
            colliderWhoHave[i].GetComponent<Collider>().enabled = true;
        }
    }

    private string dateFromPicker = "";
    private string prevDateFromPicker = "";
	
    public void GetCommentByDate()
    {
        if(dateInputField.text=="")
        {
                string now = DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day.ToString() + "/" + DateTime.Now.Year;
                
                if (cells != null)
                {
                    DestroyList();
                }
               
                StartCoroutine(GetAllMessageDateAsync(idserveruser, now));
        }
        else
        {
            if (cells != null)
            {
                DestroyList();
            }
            StartCoroutine(GetAllMessageDateAsync(idserveruser, dateInputField.text));
        }
    }
    
    void Update () 
    {
		if(cells==null)
        {
            NoCommentText.text = "No Comments...";
        }
        else
        {
            NoCommentText.text = "";
        }
        
	}

    IEnumerator getCountryImage(string cCode,RawImage tmpImage,int index)
    {
        string realURL = "http://kick2balls.com/images/flags-normal/" + cCode + ".png";
        Debug.Log("I" + index + ": " + realURL);
        WWW www = new WWW(realURL);

        while (!www.isDone)
        {
            yield return null;
        }
        
        yield return www;
        Texture2D tex = new Texture2D(www.texture.width, www.texture.height, TextureFormat.RGB24, false);
        www.LoadImageIntoTexture(tex);
        tmpImage.texture = tex;
    }

    IEnumerator GetAllMessageDateAsync(int iduser,string date)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", iduser);
        form.AddField("date", date);

        Debug.Log("ID USer: " + iduser.ToString() + " Date: " + date.ToString());

        form.AddField("functionName", "GetMessage");
        WWW w = new WWW(url, form);
        yield return w;
        string Response = w.text;
        var Result = Response.Split(new char[] { '\n' });
        Debug.Log("Rs: " + Response);

        for (int i = 0; i < Result.Length-1; i++)
        {
            Message CurMessage = new Message();
            Debug.Log("Ri: " + Result[i]);
            CurMessage = JsonUtility.FromJson<Message>(Result[i]);
            ListMessage.Add(CurMessage);
        }

        
        cells = new List<GameObject>(ListMessage.Count);

        for (int i = 0; i < ListMessage.Count; i++)
        {
            int n = i;
            cellObj = Instantiate(listPrefab, parentObject.transform);
            cellObj.GetComponent<CellObjects>().actionTitle.text = "Action: " + ListMessage[i].Action;
            cellObj.GetComponent<CellObjects>().country.text = "Country: " + ListMessage[i].countryCode.ToUpper();
            cellObj.GetComponent<CellObjects>().commentMessage.text = ListMessage[i].Msg;
            cellObj.GetComponent<CellObjects>().kickCount.text = ListMessage[i].CountKick.ToString();
            cellObj.GetComponent<CellObjects>().highCount.text = ListMessage[i].CountFive.ToString();
            cellObj.GetComponent<CellObjects>().dateText.text = ListMessage[i].MsgDate;
            cellObj.GetComponent<CellObjects>().kickButton.onClick.AddListener(delegate { kickButtonHandler(n,ListMessage[n].IdMsg);});
            cellObj.GetComponent<CellObjects>().hightButton.onClick.AddListener(delegate { highButtonHandler(n, ListMessage[n].IdMsg);});
            cellObj.GetComponent<CellObjects>().reportButton.onClick.AddListener(delegate { reportButtonHandler(n, ListMessage[n].IdMsg);});
            StartCoroutine(getCountryImage(ListMessage[i].countryCode,cellObj.GetComponent<CellObjects>().countryFlag, i));
            cells.Add(cellObj);
        }
    }

    public void UpdateList()
    {
       for (int i = 0; i < ListMessage.Count; i++)
       {
           Destroy(cells[i].gameObject);
       }
       cells.Clear();
       ListMessage.Clear();
       Destroy(cellObj);
      
       StartCoroutine(GetAllMessageDateAsync(idserveruser, "07/03/2017"));
    }

    public void DestroyList()
    {
        for (int i = 0; i < ListMessage.Count; i++)
        {
            Destroy(cells[i].gameObject);
        }
        cells.Clear();
        ListMessage.Clear();
        Destroy(cellObj);
    }

    IEnumerator MsgActionAsync(int iduser,int IdMessage,string action)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", iduser);
        form.AddField("id", IdMessage);
        if (action == "Kick2Balls")
        {
            action = "1";
        }
        else if (action == "HighFive")
        {
            action = "2";
        }
        else if(action == "No Action")
        {
            action = "3";
        }
        form.AddField("act", action);
        form.AddField("functionName", "SetMsgAction");
        WWW w = new WWW(url, form);
        yield return w;
        string Response = w.text;
        Debug.Log(Response);
    }

    IEnumerator ReportAsync(int iduser,int IdMessage)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", iduser);
        form.AddField("id", IdMessage);
        form.AddField("functionName", "Report");
        WWW w = new WWW(url, form);
        yield return w;
        string Response = w.text;
        Debug.Log(Response);
    }

    IEnumerator NotificationAsync(int iduser)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", iduser);
        form.AddField("functionName", "ReadWriteNotification");
        WWW w = new WWW(url, form);
        yield return w;
        string Response = w.text;
        Debug.Log(Response);
    }
}
