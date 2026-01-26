using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// レンタルパーティのマスターデータ
/// </summary>
[CreateAssetMenu(menuName = "Party/Rental")]
public class RentalPartySO : ScriptableObject
{
    public List<RentalCharacter> rentalCharacter = new List<RentalCharacter>();
}
