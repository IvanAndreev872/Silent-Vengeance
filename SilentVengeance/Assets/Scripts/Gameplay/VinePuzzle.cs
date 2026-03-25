using UnityEngine;

public class VinePuzzle : MonoBehaviour
{
    [SerializeField] private Vine[] vines;
    [SerializeField] private int[] correctOrder; // например: {2, 0, 1}
    [SerializeField] private GameObject door;

    private int[] _playerOrder;
    private int _currentStep = 0;

    private void Awake()
    {
        _playerOrder = new int[correctOrder.Length];
    }

    public void OnVinePulled(int index)
    {
        _playerOrder[_currentStep] = index;
        _currentStep++;

        if (_currentStep >= correctOrder.Length)
            CheckOrder();
    }

    private void CheckOrder()
    {
        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (_playerOrder[i] != correctOrder[i])
            {
                Invoke(nameof(ResetAll), 0.5f); // небольшая пауза перед сбросом
                return;
            }
        }
        Solve();
    }

    private void Solve()
    {
        Debug.Log("Головоломка решена!");
        if (door != null) door.SetActive(false);
    }

    private void ResetAll()
    {
        _currentStep = 0;
        foreach (var vine in vines)
            vine.ResetVine();
    }
}