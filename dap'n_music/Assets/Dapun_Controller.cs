using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Dapun_Controller : MonoBehaviour {

    /**レーダークラス**/
    [Serializable]
    public class Houhe_Class
    {
        public float CursorArea = 0.1f;
        public float CursorSpeed = 10f;
        public GameObject Cursor;
        public Image CursorImage;
        public Sprite CursorNormal, CursorHit;
        public GameObject Target;
        public Image TargetImage;
    }
    public Houhe_Class Houhe;

    /**括約筋クラス**/
    [Serializable]
    public class Katuyaku_Class
    {
        public float CursorArea = 0.1f;
        public GameObject Cursor;
        public Image CursorImage;
        public Sprite CursorNormal, CursorHit;
        public GameObject Target;
        public Image TargetImage;
    }
    public Katuyaku_Class Katuyaku;

    /**レーダー変数**/
    float H_nAngle = 0;    //現在の入力角度
    float H_cAngle = 0, H_cAngle2 = 0;  //現在のカーソル角度・範囲含めたカーソル角度
    float H_tAngle = 0;     //現在のターゲット角度
    bool H_ChangeFlg = false; //画像入れ替えフラグ（true･･･アタリ画像 / false･･･ハズレ画像）

    /**括約筋変数**/
    float K_cursorPos = 0;
    bool ChangeFlg = false; //画像入れ替えフラグ（true･･･アタリ画像 / false･･･ハズレ画像）

    /**共通変数**/
    bool ClickFlg = false;

    // Use this for initialization
    void Start () {
        if (Houhe.CursorImage) Houhe.CursorImage.fillAmount = Houhe.CursorArea;
        Houhe_RandomTarget();
    }
	
	// Update is called once per frame
	void Update () {
        //クリック入力
        ClickFlg = GetInputClick();
        //カーソル移動
        H_nAngle -= GetInputX();
        //レーダー処理
        Houhe_Update();
    }

    #region /**キー入力**/
    //クリック入力
    bool GetInputClick()
    {
        return Input.GetButtonDown("Fire1");
    }
    //横キー入力
    float GetInputX()
    {
        return Input.GetAxisRaw("Horizontal") * Houhe.CursorSpeed * Time.deltaTime;
    }
    #endregion

    #region /**放屁レーダー処理**/
    /*放屁レーダーアップデート*/
    void Houhe_Update()
    {
        //移動していなければスキップ
        if (H_nAngle != H_cAngle)
        {
            H_cAngle = H_nAngle;
            H_cAngle2 = H_cAngle + Houhe.CursorArea * 360;
        }
        //角度計算 & ターゲット確認フラグ
        bool flg = Houhe_TargetCalc();

        if (Houhe.Cursor == null) return;
        //画像変更
        Houhe_ChangeImage(flg);
        //ターゲット位置ランダム変更
        if (flg && ClickFlg) Houhe_RandomTarget();
    }

    /**エリア内にターゲットが入っているか**/
    bool Houhe_TargetCalc()
    {
        //角度修正（修正予定）
        if (H_nAngle < 0)
        {
            H_nAngle += 360;
        }
        else if (H_nAngle > 360)
        {
            H_nAngle -= 360;
        }
        if (H_cAngle < 0)
        {
            H_cAngle += 360;
        }
        else if (H_cAngle > 360)
        {
            H_cAngle -= 360;
        }
        if (H_cAngle2 < 0)
        {
            H_cAngle2 += 360;
        }
        else if (H_cAngle2 > 360)
        {
            H_cAngle2 -= 360;
        }
        float rotate = 0;
        //角度判定
        if (H_tAngle - Houhe.CursorArea * 360 < 0)
        {
            if (H_cAngle > H_cAngle2)
            {
                rotate = H_cAngle - 360;
            }
            else
            {
                rotate = H_cAngle;
            }
            if (rotate < H_tAngle && H_cAngle2 > H_tAngle) return true;
        }
        else if (H_tAngle + Houhe.CursorArea * 360 > 360)
        {
            if (H_cAngle > H_cAngle2)
            {
                rotate = H_cAngle2 + 360;
            }
            else
            {
                rotate = H_cAngle2;
            }
            if (H_cAngle < H_tAngle && rotate > H_tAngle) return true;
        }
        else
        {
            if (H_cAngle < H_tAngle && H_cAngle2 > H_tAngle) return true;
        }
        return false;
    }

    /**ランダム位置にターゲット出現**/
    void Houhe_RandomTarget()
    {
        H_tAngle = UnityEngine.Random.Range(0f, 360f);
        if (Houhe.Target) Houhe.Target.transform.localEulerAngles = new Vector3(0, 0, H_tAngle);
    }

    /**画像変更**/
    void Houhe_ChangeImage(bool i_flg)
    {
        Houhe.Cursor.transform.localEulerAngles = new Vector3(0, 0, H_cAngle);
        if (i_flg && !H_ChangeFlg)
        {
            if (Houhe.CursorHit == null) return;
            Houhe.CursorImage.sprite = Houhe.CursorHit;
            H_ChangeFlg = true;
        }
        else if (!i_flg && H_ChangeFlg)
        {
            if (Houhe.CursorNormal == null) return;
            Houhe.CursorImage.sprite = Houhe.CursorNormal;
            H_ChangeFlg = false;
        }
    }
    #endregion

    #region /**括約ゲージ処理**/


    #endregion
}
