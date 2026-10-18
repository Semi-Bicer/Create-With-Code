using UnityEngine;

public class DiceReader : MonoBehaviour
{
    public DiceFaceTable faceTable;

    public bool TryReadValue(out int resultValue)
    {
        if (faceTable == null || faceTable.faces.Length == 0)
        {
            resultValue = -1;
            return false;
        }

        // değerleri resetlemek için burada tanımlandılar, global yerine. Kullanıcı tekrar zar atarsa bu değerlerin resetlenip tekrar hesaplanması gerekiyor.
        float bestDot = -2.0f; //Dot product values range between -1.0 and 1.0, so -2.0 guarantees the first face checked will be higher.
        int bestValue = -1;

        // d4 dışındaki zarlar yukarı bakan yüzü okur. Start yerine burada hesaplanıyor:
        // Start sadece Play mode'da çalışır, bu sayede context menu edit mode'da da doğru sonuç veriyor.
        Vector3 targetReference = faceTable.readsDownwardFace ? Vector3.down : Vector3.up;

        foreach (var face in faceTable.faces)
        {
            Vector3 worldDirection = transform.rotation * face.faceValueDirection;
            //dot product
            float currentDot = Vector3.Dot(targetReference, worldDirection);
            if (currentDot > bestDot)
            {
                bestDot = currentDot;
                bestValue = face.faceValue;
            }
        }

        if (bestDot > faceTable.cockedThreshold)
        {
            resultValue = bestValue;
            return true;
        }
        else
        {
            resultValue = -1;
            return false;
        }
    }

    [ContextMenu("ReadNow")]
    public void TestRead()
    {
        if (TryReadValue(out int rolledValue))
        {
            Debug.Log($"Success! Rolled value: {rolledValue}");
        }
        else
        {
            Debug.Log($"Dice is tilted / cocked!");
        }
    }
}
