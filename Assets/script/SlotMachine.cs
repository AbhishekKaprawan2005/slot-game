using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    public SlotReel reel1;
    public SlotReel reel2;
    public SlotReel reel3;

    public PrizeManager prizeManager;
    public Button spinButton;

    private bool isSpinning;


    void Start()
    {
        // Connect the Spin button to the method that starts the slot machine.
        if (spinButton != null)
            spinButton.onClick.AddListener(OnSpinPressed);
    }


    public void OnSpinPressed()
    {
        // Prevent the player from starting another spin
        // while the current spin is still running.
        if (isSpinning) return;

        // Check whether the player has a free spin available.
        bool freeSpin = prizeManager != null && prizeManager.HasFreeSpin();

        // A normal spin requires enough balance to pay the current bet.
        // Free spins do not require the player to pay a bet.
        if (!freeSpin && prizeManager != null && !prizeManager.CanAffordBet())
        {
            // Not enough balance to spin again, and no free spin available.
            return;
        }

        StartCoroutine(SpinRoutine(freeSpin));
    }


    private System.Collections.IEnumerator SpinRoutine(bool freeSpin)
    {
        // Lock the spin state so another spin cannot be started
        // until all three reels have stopped.
        isSpinning = true;

        if (spinButton != null)
            spinButton.interactable = false;


        if (prizeManager != null)
        {
            // Free spins are consumed without deducting money.
            if (freeSpin)
                prizeManager.UseFreeSpin();

            // Normal spins deduct the selected bet from the balance.
            else
                prizeManager.PlaceBet();
        }


        // Each reel has a different duration so they stop one after another,
        // creating a more realistic slot-machine effect.
        reel1.StartSpin(1.0f);
        reel2.StartSpin(1.4f);
        reel3.StartSpin(1.8f);


        // Wait until all three reels have completely stopped.
        // The result should only be evaluated after every reel has finished.
        while (reel1.IsSpinning ||
               reel2.IsSpinning ||
               reel3.IsSpinning)
        {
            yield return null;
        }


        // Pass the final symbol index of each reel to PrizeManager.
        // PrizeManager then checks for matching symbols and calculates the reward.
        if (prizeManager != null)
        {
            prizeManager.EvaluateSpin(
                reel1.CurrentIndex,
                reel2.CurrentIndex,
                reel3.CurrentIndex
            );
        }


        // Allow the player to start another spin after the result is calculated.
        if (spinButton != null)
            spinButton.interactable = true;

        isSpinning = false;
    }
}