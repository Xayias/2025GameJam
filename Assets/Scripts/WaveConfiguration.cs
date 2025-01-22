using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveConfiguration", menuName = "WaveSystem/WaveConfiguration")]
public class WaveConfiguration : ScriptableObject
{
    public string waveName; // Name for the wave (e.g., "Wave 1")
    public int[] enemyCounts;   // Array: Number of enemies to spawn for each enemy type
}
