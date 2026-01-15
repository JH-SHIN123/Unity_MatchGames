using UnityEngine;

public class Gem : MonoBehaviour
{
	// [Unreal: UPROPERTY() UGemData* Data;]
	[HideInInspector] public GemData data;

	private int _gemGrid;
	private SpriteRenderer _spriteRenderer;
	private Vector3 _newPosition = Vector3.zero;
	private bool _activeMove = false;
	private float _moveTime = 0.0f;

	public int GetGemGrid() { return _gemGrid; }
	public void SetGemGrid(int newGrid) { _gemGrid = newGrid; }
	public void MoveGem(Vector3 NewPosition)
	{
		_moveTime = 0.0f;
		_activeMove = true;
		_newPosition = NewPosition;
	}

	private void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	public void Initialize(GemData newData, int gewGrid)
	{
		data = newData;
		_gemGrid = gewGrid;

		if (_spriteRenderer != null)
		{
			_spriteRenderer.sprite = data.icon; // 이미지 교체
			_spriteRenderer.color = data.color; // 색상(틴트) 교체
		}

		this.name = data.gemName;
	}

	public void Update()
	{
		if(_activeMove)
		{
			float duration = 0.75f;

			if (_moveTime < duration)
			{
				_moveTime += Time.deltaTime;
				float t = _moveTime / duration;
				transform.position = Vector3.Lerp(transform.position, _newPosition, t);
			}
		}
	}
}