using System.Collections;
using UnityEngine;

public class SlotReel : MonoBehaviour
{
    [Header("8 Visual Symbols")]
    [SerializeField] private Transform[] symbols;

    [Header("Symbol IDs")]
    [Tooltip("0 = Bell, 1 = Cherry, 2 = Seven, 3 = BAR")]
    [SerializeField] private int[] symbolIDs;

    [Header("Settings")]
    [SerializeField] private float symbolSpacing = 1.5f;
    [SerializeField] private float bottomLimit = -6f;
    [SerializeField] private float spinSpeed = 12f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinSound;
    [SerializeField] private AudioClip stopSound;

    private bool isSpinning;
    private int currentIndex;

    public bool IsSpinning => isSpinning;
    public int CurrentIndex => currentIndex;


    public void StartSpin(float duration)
    {
        if (!isSpinning)
            StartCoroutine(Spin(duration));
    }


    private IEnumerator Spin(float duration)
    {
        isSpinning = true;

        // Play the spinning sound when the reel starts moving.
        if (audioSource != null && spinSound != null)
        {
            audioSource.clip = spinSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Select a random visual sprite.
        int visualIndex = Random.Range(0, symbols.Length);

        // Convert visual index into the actual logical symbol ID.
        currentIndex = symbolIDs[visualIndex];

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            MoveSymbols();

            yield return null;
        }

        StopAtSymbol(visualIndex);

        // Stop the spinning sound when the reel stops.
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Play a short sound when the reel reaches its result.
        if (audioSource != null && stopSound != null)
        {
            audioSource.PlayOneShot(stopSound);
        }

        Debug.Log(
            gameObject.name +
            " RESULT = " + GetSymbolName(currentIndex) +
            " | ID = " + currentIndex
        );

        isSpinning = false;
    }


    private void MoveSymbols()
    {
        for (int i = 0; i < symbols.Length; i++)
        {
            Vector3 pos = symbols[i].localPosition;

            pos.y -= spinSpeed * Time.deltaTime;

            symbols[i].localPosition = pos;
        }

        float highestY = GetHighestY();

        for (int i = 0; i < symbols.Length; i++)
        {
            if (symbols[i].localPosition.y <= bottomLimit)
            {
                Vector3 pos = symbols[i].localPosition;

                pos.y = highestY + symbolSpacing;

                symbols[i].localPosition = pos;

                highestY = pos.y;
            }
        }
    }


    private float GetHighestY()
    {
        float highestY = symbols[0].localPosition.y;

        for (int i = 1; i < symbols.Length; i++)
        {
            if (symbols[i].localPosition.y > highestY)
                highestY = symbols[i].localPosition.y;
        }

        return highestY;
    }


    private void StopAtSymbol(int index)
    {
        // Shift the complete reel so the selected symbol
        // is positioned at the center.
        float difference = -symbols[index].localPosition.y;

        for (int i = 0; i < symbols.Length; i++)
        {
            Vector3 pos = symbols[i].localPosition;

            pos.y += difference;

            symbols[i].localPosition = pos;
        }
    }


    private string GetSymbolName(int id)
    {
        switch (id)
        {
            case 0: return "Bell";
            case 1: return "Cherry";
            case 2: return "Seven";
            case 3: return "BAR";
            default: return "Unknown";
        }
    }
}