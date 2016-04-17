using UnityEngine;
using System.Collections;

public class SpriteAnimation : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] AnimationCells;
    [SerializeField] float FrameDelay = 0.2f;
    [SerializeField] int currentFrame;
    [SerializeField] bool Playing;
    [SerializeField] bool OneShotting;

    float animElapsed;
  
    public bool IsPlaying
    {
        get
        {
            return Playing;
        }
    }

    void Start()
    {
    }
    
    void Update()
    {
        if (Playing)
        {
            animElapsed += Time.deltaTime;
            if (animElapsed >= AnimationCells.Length * FrameDelay)
            {
                animElapsed = 0;

                //Stop the animation at end if this is a one-shot play
                if (OneShotting)
                {
                    Playing = false;
                    OneShotting = false;
                }
            }
            else
            {
                //Update current animation frame
                currentFrame = (int)(animElapsed / FrameDelay);
                spriteRenderer.sprite = AnimationCells[currentFrame];
            }
        }
    }

    public void Play()
    {
        Playing = true;
        OneShotting = false;
    }

    public void Stop()
    {
        Playing = false;
        OneShotting = false;
    }

    public void PlayOneShot()
    {
        Playing = true;
        OneShotting = true;
        animElapsed = 0;
    }
}
