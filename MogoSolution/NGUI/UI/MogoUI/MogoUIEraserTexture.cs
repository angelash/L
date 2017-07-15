using UnityEngine;
using System.Collections;

public class MogoUIEraserTexture : MonoBehaviour {
    struct EraserPoint
    {

        public int x;
        public int y;
        public bool flag;

        public EraserPoint(int _x, int _y, bool _flag)
        {
            this.x = _x;
            this.y = _y;
            this.flag = _flag;
        }

        public override string ToString()
        {
            return string.Format("[EraserPoint: x={0}, y={1}, flag={2}]", x, y, flag);
        }

    }

    public UITexture image;
    public int brushScale = 4;

    GameObject mEventTarget;
    string mEventHander;

    bool isGame = false;

    int ctx = 0;
    int cty = 0;

    Texture2D texRender;
    Camera canvas;

    public void SetTarget(GameObject _target, string _hander)
    {
        this.mEventTarget = _target;
        this.mEventHander = _hander;
    }

    void Awake()
    {
        canvas = NGUITools.FindCameraForLayer(gameObject.layer);
        if (image == null)
            image = GetComponent<UITexture>();
    }

    void Start()
    {
        texRender = new Texture2D(image.mainTexture.width, image.mainTexture.height, TextureFormat.ARGB32, true);
        ctx = texRender.width / 2;
        cty = texRender.height / 2;
        Reset();
    }

    void OnPress(bool isPressed)
    {
        if (isGame == false)
            return;
        start = ConvertSceneToUI(Input.mousePosition);
        OnMouseMove(start);
    }

    void OnDrag(Vector2 delta)
    {
        if (isGame == false)
            return;
        start += delta;
        OnMouseMove(start);
    }

    Vector2 start = Vector2.zero;

    Vector2 ConvertSceneToUI(Vector3 posi)
    {
        Vector2 postion;
        postion = canvas.ScreenToWorldPoint(posi);
        postion = transform.InverseTransformPoint(postion);
        return postion;
    }

    public void OnMouseMove(Vector2 end)
    {
        Rect rect = new Rect(end.x + (texRender.width - brushScale) / 2, end.y + (texRender.height - brushScale) / 2, brushScale, brushScale);
        Draw(rect);
    }

    public void Reset()
    {

        if (texRender == null)
            return;

        for (int i = 0; i < texRender.width; i++)
        {

            for (int j = 0; j < texRender.height; j++)
            {

                Color color = texRender.GetPixel(i, j);
                color.a = 1;
                texRender.SetPixel(i, j, color);

            }
        }

        texRender.Apply();

        image.material.SetTexture("_RendTex", texRender);

        isOver = false;
        isGame = false;

    }

    void Draw(Rect rect)
    {

        for (int x = (int)rect.xMin; x < (int)rect.xMax; x++)
        {
            for (int y = (int)rect.yMin; y < (int)rect.yMax; y++)
            {
                if (x < 0 || x > texRender.width || y < 0 || y > texRender.height)
                {
                    return;
                }
                Color color = texRender.GetPixel(x, y);
                if (color.a == 0)
                {
                    continue;
                }
                color.a = 0;
                texRender.SetPixel(x, y, color);
            }
        }

        texRender.Apply();
        image.material.SetTexture("_RendTex", texRender);

        if (!isOver)
        {
            isOver = IsOver();
        }

        if (isOver)
        {

            isGame = false;

            int minx = mDataPoints[0].x - 8;
            int miny = mDataPoints[0].y - 8;
            int maxx = mDataPoints[8].x + 8;
            int maxy = mDataPoints[8].y + 8;

            for (int x = minx; x <= maxx; x++)
            {
                for (int y = miny; y <= maxy; y++)
                {
                    Color color = texRender.GetPixel(x, y);
                    if (color.a == 0)
                    {
                        continue;
                    }
                    color.a = 0;
                    texRender.SetPixel(x, y, color);
                }
            }

            texRender.Apply();
            image.material.SetTexture("_RendTex", texRender);

            if (mEventTarget != null && !string.IsNullOrEmpty(mEventHander))
            {
                mEventTarget.SendMessage(mEventHander, SendMessageOptions.DontRequireReceiver);
            }
            Debug.Log("isOver.");
        }
    }

    bool isOver = false;

    EraserPoint[] mDataPoints = new EraserPoint[9];

    public void Set(int w, int h)
    {

        int w2 = w / 2 - 8;
        int h2 = h / 2 - 8;

        mDataPoints[0] = new EraserPoint(ctx - w2, cty - h2, false);
        mDataPoints[1] = new EraserPoint(ctx - w2, cty, false);
        mDataPoints[2] = new EraserPoint(ctx - w2, cty + h2, false);

        mDataPoints[3] = new EraserPoint(ctx, cty - h2, false);
        mDataPoints[4] = new EraserPoint(ctx, cty, false);
        mDataPoints[5] = new EraserPoint(ctx, cty + h2, false);

        mDataPoints[6] = new EraserPoint(ctx + w2, cty - h2, false);
        mDataPoints[7] = new EraserPoint(ctx + w2, cty, false);
        mDataPoints[8] = new EraserPoint(ctx + w2, cty + h2, false);

        Reset();

        isGame = true;
    }

    public bool IsOver()
    {
        for (int i = 0; i < mDataPoints.Length; i++)
        {
            EraserPoint point = mDataPoints[i];
            if (point.flag)
                continue;
            bool ret = false;
            for (int x = point.x - 4; x <= point.x + 4; x++)
            {
                for (int y = point.y - 4; y <= point.y + 4; y++)
                {
                    if (texRender.GetPixel(x, y).a == 0)
                    {
                        mDataPoints[i].flag = ret = true;
                        break;
                    }
                }
                if (ret) break;
            }
            if (!ret) return false;
        }
        return true;
    }
}
