using System;
using System.Collections;
using UnityEngine;

public class FadeInAndFadeOut : MonoBehaviour
{
    [SerializeField] GameObject cube;
    [SerializeField] float timeDurationViible = 20f;

     Vector3 smoothVelocity = Vector3.zero;
    float smoothDurtion = 0.5f;
    void Start()
    {
        if(cube == null)
        {
            return;
        }

        cube.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cube.SetActive(true);   
            StartCoroutine(VisibleAndInvisible(cube , timeDurationViible));
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(ScaleDownScaleUp(cube , timeDurationViible));
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("D is press");
            StartCoroutine(FadeOff(cube , timeDurationViible));
        }
    }


  
    IEnumerator VisibleAndInvisible(GameObject obj , float duration)
    {
        Renderer rend  =  obj.GetComponent<Renderer>();

        if (rend == null)
        {

            Debug.Log(" rendered is not found in the gameObject ");
            yield break;

        }


        Material mat = rend.material;
        Color color = mat.color;  // Get current color
        color.a = 0f;              //Change alpha to 0 (invisible)
        mat.color = color;       // Apply new color to material

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t/duration);
            color.a = alpha;
            mat.color = color;   
            
            yield return null;
        }

        color.a = 1f;
        mat.color = color; 
    }


    IEnumerator FadeOff(GameObject obj, float duration)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.Log("Renderer is not available");
            yield break;
        }

        Material mat = rend.material;
        Color fadeCol = mat.color;

        // Start fully visible
        fadeCol.a = 1f;
        mat.color = fadeCol;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);

            fadeCol.a = alpha;
            mat.color = fadeCol; // VERY IMPORTANT

            yield return null;
        }
    }



    IEnumerator ScaleDownScaleUp(GameObject obj, float duration)
    {
        Transform tran = obj.transform;

        Vector3 originalScale = tran.localScale;
        Vector3 targetScale = originalScale * 5f;

        Vector3 currentScale = originalScale;
        Vector3 smoothVelocity = Vector3.zero;

        float halfDuration = duration / 2f;

        // --- SCALE UP ---
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;

            currentScale = Vector3.SmoothDamp(
                currentScale,
                targetScale,
                ref smoothVelocity,
                halfDuration
            );

            tran.localScale = currentScale;
            yield return null;
        }
    }

}
