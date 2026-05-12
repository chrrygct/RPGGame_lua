using System;
using UnityEngine;
using RPG.Saving;

namespace RPG.Inventories
{
    /// <summary>
    /// Holds the player's money. Place on the GameObject tagged "Player".
    /// </summary>
    public class Purse : MonoBehaviour, ISaveable
    {
        [SerializeField] float startingBalance = 0f;

        float balance = 0f;

        public event Action onChange;

        private void Awake()
        {
            balance = startingBalance;
        }

        public float GetBalance()
        {
            return balance;
        }

        public void UpdateBalance(float amount)
        {
            balance += amount;
            if (onChange != null)
            {
                onChange();
            }
        }

        object ISaveable.CaptureState()
        {
            return balance;
        }

        void ISaveable.RestoreState(object state)
        {
            balance = (float)state;
            if (onChange != null)
            {
                onChange();
            }
        }
    }
}
