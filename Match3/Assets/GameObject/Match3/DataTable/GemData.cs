using UnityEngine;

[CreateAssetMenu(fileName = "GemData", menuName = "Scriptable Objects/GemData")]
public class GemData : ScriptableObject
{
	[Header("Basic Info")]
	public string gemName;     // 보석 이름 (Red, Blue...)
	public Sprite icon;        // 사용할 이미지
	public Color color;        // 사용할 이미지
	public int scoreValue;     // 터졌을 때 점수

	[Header("Audio")]
	public AudioClip popSound; // 터질 때 소리
}
