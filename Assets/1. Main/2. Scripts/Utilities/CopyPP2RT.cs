using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copy PostProcess To Render Texture
public class CopyPP2RT : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] RenderTexture _renderTexture;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // src: 후처리까지 포함된 최종 화면
        // renderTexture에 복사
        Graphics.Blit(src, _renderTexture);

        // 원래대로 화면에도 출력
        Graphics.Blit(src, dest);
    }
}
