using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrizeManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text balanceText;
    public TMP_Text prizeText;
    public TMP_Text betText;

    public Button increaseBetButton;
    public Button decreaseBetButton;
    public Button jackpotButton;

    [Header("Money")]
    public int startingBalance = 100;

    [Header("Bet")]
    public int betAmount = 10;
    public int betIncrease = 10;
    public int minBet = 10;
    public int maxBet = 50;

    [Header("3 Match Prizes")]
    // Index represents the symbol:
    // 0 = Bell, 1 = Cherry, 2 = Seven, 3 = BAR.
    // The prize is selected using the symbol's index.
    public int[] threeMatchPrizes = { 50, 80, 150, 300 };

    [Header("2 Match Prize")]
    public int twoMatchPrize = 20;

    [Header("Bonus")]
    public int freeSpinsAwarded = 3;
    public int jackpotBonusAmount = 500;
    public float jackpotButtonDuration = 2f;

    private int balance;
    private int freeSpins = 0;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip jackpotSound;
    private void Start()
    {
        // Initialize the player's balance when the game starts.
        balance = startingBalance;

        UpdateBalanceText();
        UpdateBetText();

        if (prizeText != null)
            prizeText.text = "";

        // Connect the UI buttons to their corresponding methods.
        if (increaseBetButton != null)
        {
            increaseBetButton.onClick.AddListener(IncreaseBet);
        }

        if (decreaseBetButton != null)
        {
            decreaseBetButton.onClick.AddListener(DecreaseBet);
        }

        if (jackpotButton != null)
        {
            // Jackpot button is hidden until a 3-symbol match occurs.
            jackpotButton.gameObject.SetActive(false);
            jackpotButton.onClick.AddListener(OnJackpotButtonClicked);
        }
    }


    #region BET

    public void IncreaseBet()
    {
        if (betAmount < maxBet)
        {
            betAmount += betIncrease;

            // Prevent the bet from going above the maximum allowed value.
            if (betAmount > maxBet)
                betAmount = maxBet;

            UpdateBetText();
        }
    }


    public void DecreaseBet()
    {
        if (betAmount > minBet)
        {
            betAmount -= betIncrease;

            // Prevent the bet from going below the minimum allowed value.
            if (betAmount < minBet)
                betAmount = minBet;

            UpdateBetText();
        }
    }


    private void UpdateBetText()
    {
        if (betText != null)
            betText.text = "BET " + betAmount;
    }

    #endregion


    #region FREE SPIN

    public bool HasFreeSpin()
    {
        return freeSpins > 0;
    }


    public void UseFreeSpin()
    {
        // A free spin is consumed only when at least one is available.
        if (freeSpins > 0)
            freeSpins--;
    }

    #endregion


    #region BALANCE

    public bool CanAffordBet()
    {
        // The player can spin only if their balance covers the current bet.
        return balance >= betAmount;
    }


    public void PlaceBet()
    {
        // Deduct the selected bet before the reels start spinning.
        balance -= betAmount;

        UpdateBalanceText();

        if (prizeText != null)
            prizeText.text = "Spinning...";
    }

    #endregion


    #region PRIZE SYSTEM

    public void EvaluateSpin(int s1, int s2, int s3)
    {
        Debug.Log("RESULT " + s1 + " | " + s2 + " | " + s3);

        // First determine how many of the three symbols are identical.
        int matches = CountMatches(s1, s2, s3);

        // The prize depends on the number of matches.
        // For three matches, s1 is used to identify the winning symbol.
        int prize = GetPrize(matches, s1);

        // Add the calculated prize to the player's balance.
        balance += prize;

        UpdateBalanceText();


        // Three identical symbols produce the highest reward
        // and also award additional free spins.
        if (matches == 3)
        {
            freeSpins += freeSpinsAwarded;

            if (prizeText != null)
            {
                prizeText.text =
                    "JACKPOT! +" + prize +
                    "\n+" + freeSpinsAwarded + " FREE SPINS";
            }

            // Temporarily display the bonus jackpot button.
            StartCoroutine(ShowJackpotButtonRoutine());
        }


        // Exactly two identical symbols give a smaller fixed reward.
        else if (matches == 2)
        {
            if (prizeText != null)
            {
                prizeText.text =
                    "2 MATCH!\n+" + prize;
            }
        }


        // No two or three symbols match, so no prize is awarded.
        else
        {
            if (prizeText != null)
                prizeText.text = "NO WIN";
        }
        if (matches == 3)
        {
            freeSpins += freeSpinsAwarded;

            if (audioSource != null && jackpotSound != null)
                audioSource.PlayOneShot(jackpotSound);

            if (prizeText != null)
            {
                prizeText.text =
                    "JACKPOT! +" + prize +
                    "\n+" + freeSpinsAwarded + " FREE SPINS";
            }

            StartCoroutine(ShowJackpotButtonRoutine());
        }
        else if (matches == 2)
        {
            if (audioSource != null && winSound != null)
                audioSource.PlayOneShot(winSound);

            if (prizeText != null)
            {
                prizeText.text =
                    "2 MATCH!\n+" + prize;
            }
        }
    }


    private int CountMatches(int s1, int s2, int s3)
    {
        // Check three-of-a-kind first because it also satisfies
        // the condition that at least two symbols match.
        if (s1 == s2 && s2 == s3)
            return 3;

        // Any matching pair counts as a two-symbol match.
        if (s1 == s2 ||
            s2 == s3 ||
            s1 == s3)
            return 2;

        return 0;
    }


    private int GetPrize(int matchCount, int matchedSymbol)
    {
        if (matchCount == 3)
        {
            // The symbol index is used to retrieve its corresponding
            // payout from the threeMatchPrizes array.
            if (matchedSymbol >= 0 &&
                matchedSymbol < threeMatchPrizes.Length)
            {
                return threeMatchPrizes[matchedSymbol];
            }

            // Safety check in case an invalid symbol index is received.
            return 0;
        }

        // Two matching symbols always receive the same fixed prize.
        if (matchCount == 2)
            return twoMatchPrize;

        return 0;
    }

    #endregion


    #region JACKPOT

    private IEnumerator ShowJackpotButtonRoutine()
    {
        if (jackpotButton == null)
            yield break;

        jackpotButton.gameObject.SetActive(true);

        // Keep the bonus button visible only for the configured duration.
        yield return new WaitForSeconds(jackpotButtonDuration);

        jackpotButton.gameObject.SetActive(false);
    }


    private void OnJackpotButtonClicked()
    {
        // Award the additional jackpot bonus when the player
        // successfully presses the temporary bonus button.
        balance += jackpotBonusAmount;

        UpdateBalanceText();

        if (prizeText != null)
        {
            prizeText.text =
                "BONUS! +" + jackpotBonusAmount;
        }

        jackpotButton.gameObject.SetActive(false);
    }

    #endregion


    private void UpdateBalanceText()
    {
        if (balanceText != null)
            balanceText.text = "BALANCE\n" + balance;
    }


    public int GetBalance()
    {
        return balance;
    }
}