using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
public class Dapun_Controller : MonoBehaviour {

    /**レーダークラス**/
    [Serializable]
    public class Dapun_Class
    {
        public float GuageMax = 100f;
        public float StartAdd = 1f;
        public float Accele = 0.5f;
        public float AcceleTime = 10f;
        public float MissSpeedAdd = 0.1f;
        public Image GuageImage;
    }
    public Dapun_Class Dapun;

    /**レーダークラス**/
    [Serializable]
    public class Houhe_Class
    {
        public float CursorArea = 0.1f;
        public float CursorSpeed = 10f;
        public float LimitTime = 5f;
        public float MissSpeedAdd = 1f;
        public float SuccessGuageAdd = -5, MissGuageAdd = 20f;
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
        public float CursorSpeed = 0.5f;
        public float CursorAccele = 0.01f;
        public float TargetArea1 = 10, TargetArea2 = 30;
        public float JustSpeedAdd = 0.1f, HitSpeedAdd = 0.1f, MissSpeedAdd = -0.5f;
        public float JustGuageAdd = -4, HitGuageAdd = -2, MissGuageAdd = 8f;
        public RectTransform CursorRect;
        public Slider CursorSlider;
        public RectTransform TargetImage1, TargetImage2;
    }
    public Katuyaku_Class Katuyaku;
    
    /**音クラス**/
    public AudioSource Audio;
    [Serializable]
    public class SoundEffect_Class
    {
        public AudioClip[] Just;
        public AudioClip[] Hit;
        public AudioClip[] Miss;
        public AudioClip[] Bar;
    }
    public SoundEffect_Class SE;

    /**脱糞ゲージ変数**/
    float D_GuageNum = 0;
    float D_AddNum = 0;

    /**放屁レーダー変数**/
    float H_nAngle = 0;    //現在の入力角度
    float H_cAngle = 0, H_cAngle2 = 0;  //現在のカーソル角度・範囲含めたカーソル角度
    float H_tAngle = 0;     //現在のターゲット角度
    bool H_ChangeFlg = false; //画像入れ替えフラグ（true･･･アタリ画像 / false･･･ハズレ画像）

    /**括約筋変数**/
    float K_cursorNum = 0;
    float K_cursorValue = 0;
    float K_nowSpeed = 0;
    bool MoveFlg = false;   //カーソル移動反転フラグ

    /**共通変数**/
    bool ClickFlg = false;

    // Use this for initialization
    void Start () {
        if (Dapun.GuageImage)
        {
            Dapun.GuageImage.fillAmount = 0;
            D_AddNum = Dapun.StartAdd;
        }
        if (Houhe.CursorImage) Houhe.CursorImage.fillAmount = Houhe.CursorArea;
        if(Katuyaku.CursorRect)
        {
            if (Katuyaku.TargetImage1) Katuyaku.TargetImage1.sizeDelta = new Vector2(-Katuyaku.CursorRect.sizeDelta.x + (Katuyaku.CursorRect.sizeDelta.x / 100) * Katuyaku.TargetArea1, 0);
            if (Katuyaku.TargetImage2) Katuyaku.TargetImage2.sizeDelta = new Vector2(-Katuyaku.CursorRect.sizeDelta.x + (Katuyaku.CursorRect.sizeDelta.x / 100) * Katuyaku.TargetArea2, 0);
        }
        K_nowSpeed = Katuyaku.CursorSpeed;
        Houhe_RandomTarget();
        //ゲームスタート
        House_MissTimeStart();
        Dapun_Start();
    }
	
	// Update is called once per frame
	void Update () {
        //クリック入力
        ClickFlg = GetInputClick();
        //カーソル移動
        H_nAngle -= GetInputX();
        //脱糞ゲージ処理
        Dapun_Update();
        //放屁レーダー処理
        Houhe_Update();
        //括約ゲージ処理
        Katuyaku_Update();
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

    #region /**脱糞ゲージ**/
    void Dapun_Update()
    {
        Dapun_GuageAdd(D_AddNum * Time.deltaTime);
    }

    void Dapun_Start()
    {
        InvokeRepeating("Dapun_AddAccele", Dapun.AcceleTime, Dapun.AcceleTime);
    }

    void Dapun_AddAccele()
    {
        Dapun_SpeedAdd(Dapun.Accele);
    }

    void Dapun_MissGuageAdd()
    {
        Dapun_SpeedAdd(Dapun.MissSpeedAdd);
    }

    void Dapun_GuageAdd(float i_add)
    {
        D_GuageNum += i_add;
        Dapun.GuageImage.fillAmount = D_GuageNum / 100; //ゲージ更新
        if (D_GuageNum < 0) D_GuageNum = 0;
    }

    void Dapun_SpeedAdd(float i_add)
    {
        D_AddNum += i_add;
        if (D_GuageNum > 100) GameOver();
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
        if (ClickFlg)
        {
            if (flg)
            {
                Dapun_GuageAdd(Houhe.SuccessGuageAdd);   //放屁成功 脱糞ゲージ減少

                House_MissTimeStart();
                CancelInvoke("Houhe_MissTimeAdd");
                InvokeRepeating("Houhe_MissTimeAdd", Houhe.LimitTime, Houhe.LimitTime);
            }
            else
            {
                Dapun_SpeedAdd(Houhe.MissSpeedAdd);   //放屁ミス 脱糞ゲージ増加量+
                Dapun_GuageAdd(Houhe.MissGuageAdd);   //放屁ミス 脱糞ゲージ増加
                Dapun_MissGuageAdd();   //ミス 脱糞ゲージ増加量+
            }
            Houhe_RandomTarget();
        }
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

    void House_MissTimeStart()
    {
        InvokeRepeating("Houhe_MissTimeAdd", Houhe.LimitTime, Houhe.LimitTime);
    }

    void Houhe_MissTimeAdd()
    {
        Dapun_GuageAdd(Houhe.MissGuageAdd);   //放屁時間切れ 脱糞ゲージ増加
    }
    #endregion

    #region /**括約ゲージ処理**/
    void Katuyaku_Update()
    {
        Katuyaku_Cursor();
        if (ClickFlg) Katuyaku_Click();
    }

    //カーソル移動処理
    void Katuyaku_Cursor()
    {
        int random;
        if (MoveFlg)
        {
            K_cursorNum += K_nowSpeed * Time.deltaTime;
            if (K_cursorNum >= 1)
            {
                MoveFlg = false;
                if (Audio && SE.Bar.Length > 0)
                {   //カーソル端SE
                    random = UnityEngine.Random.Range(0, SE.Bar.Length);
                    Audio.PlayOneShot(SE.Bar[random]);
                }
            }
        }
        else
        {
            K_cursorNum -= K_nowSpeed * Time.deltaTime;
            if (K_cursorNum <= 0)
            {
                MoveFlg = true;
                if (Audio && SE.Bar.Length > 0)
                {   //カーソル端SE
                    random = UnityEngine.Random.Range(0, SE.Bar.Length);
                    Audio.PlayOneShot(SE.Bar[random]);
                }
            }
        }
        K_cursorValue = K_cursorNum - 0.5f;
        if (Katuyaku.CursorSlider) Katuyaku.CursorSlider.value = K_cursorNum - 0.5f;

    }

    void Katuyaku_Click()
    {
        int random = 0;
        if(Mathf.Abs(K_cursorValue) < Katuyaku.TargetArea1 / 200)
        {
            Debug.Log("Jack Pot!!");
            K_nowSpeed += Katuyaku.CursorAccele;
            Dapun_SpeedAdd(Katuyaku.JustSpeedAdd);
            Dapun_GuageAdd(Katuyaku.JustGuageAdd);
            if (Audio && SE.Just.Length > 0)
            {   //ジャスト放屁音
                random = UnityEngine.Random.Range(0, SE.Just.Length);
                Audio.PlayOneShot(SE.Just[random]);
            } 
        }
        else if(Mathf.Abs(K_cursorValue) < Katuyaku.TargetArea2 / 200)
        {
            Debug.Log("Hit!!");
            K_nowSpeed += Katuyaku.CursorAccele;
            Dapun_SpeedAdd(Katuyaku.HitSpeedAdd);
            Dapun_GuageAdd(Katuyaku.HitGuageAdd);
            if (Audio && SE.Hit.Length > 0)
            {   //ヒット放屁音
                random = UnityEngine.Random.Range(0, SE.Hit.Length);
                Audio.PlayOneShot(SE.Hit[random]);
            }
        }
        else
        {
            Debug.Log("ﾌﾞｯﾁｯﾁﾌﾞﾘﾘｨ!!");
            Dapun_SpeedAdd(Katuyaku.MissSpeedAdd);
            Dapun_GuageAdd(Katuyaku.MissGuageAdd);
            if (Audio && SE.Miss.Length > 0)
            {   //ミス放屁音
                random = UnityEngine.Random.Range(0, SE.Miss.Length);
                Audio.PlayOneShot(SE.Miss[random]);
            }
        }

    }
    #endregion

    void GameOver()
    {

    }
}
