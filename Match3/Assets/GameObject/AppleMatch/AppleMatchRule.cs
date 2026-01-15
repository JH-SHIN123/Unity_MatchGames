using UnityEngine;

public class AppleMatchRule : MonoBehaviour
{
	const int MOUSE_LEFT_CLICK = 0;

	[Header("Grid Settings")]
	[SerializeField] private int width = 17;   // 가로 크기
	[SerializeField] private int height = 10;  // 세로 크기
	[SerializeField] private float cellSize = 0.5f; // 간격

	[Header("References")]
	[SerializeField] private GameObject applePrefab; // 사과
	[SerializeField] private GameObject appleUIPrefab; // 사과 UI
	[SerializeField] private GameObject dragUIRectanglePrefab; // 드래그 표시용

	private Apple[] _appleList;
	private GameObject _dragUIRectangle;

	[Header("Score")]
	[SerializeField] private int _appleMatchScore = 0;

	private Vector3 _mouseDragStart = Vector3.zero;
	private Vector3 _mouseDragEnd = Vector3.zero;
	private bool _mouseDraging = false;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		transform.position = new Vector3(width, height, 0.0f) * -0.5f;

		_appleList = new Apple[width * height];

		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				Vector3 spawnPos = new Vector3(x, y, 0.0f);
				//spawnPos += transform.position;
				spawnPos *= cellSize;

				int currentIndex = y * width + x;
				int randomAppleValue = UnityEngine.Random.Range(1, 9);

				GameObject spawnApple = Instantiate(applePrefab, spawnPos, Quaternion.identity);
				spawnApple.transform.SetParent(this.transform, false);

				Apple appleComponent = spawnApple.GetComponent<Apple>();
				if (appleComponent != null)
				{
					appleComponent.Initialize(randomAppleValue, appleUIPrefab);
				}

				// 배열에 저장
				_appleList[currentIndex] = appleComponent;

			}
		}

		_dragUIRectangle = Instantiate(dragUIRectanglePrefab, transform.position, Quaternion.identity);
		if (null != _dragUIRectangle)
		{
			_dragUIRectangle.transform.SetParent(this.transform, false);
			_dragUIRectangle.SetActive(false);
			Drag drag = _dragUIRectangle.GetComponent<Drag>();

			if (null != drag)
				drag.OnAppleMatched += OnMatchScore;

		}
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetMouseButtonDown(MOUSE_LEFT_CLICK))
			OnClickStart();

		if(_mouseDraging)
		{
			_mouseDragEnd = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_mouseDragEnd.z = -Camera.main.transform.position.z;
			//_mouseDragEnd += transform.position;

			Vector3 RightTop = new Vector3(
				_mouseDragStart.x > _mouseDragEnd.x ? _mouseDragStart.x : _mouseDragEnd.x,
				_mouseDragStart.y > _mouseDragEnd.y ? _mouseDragStart.y : _mouseDragEnd.y,
				0.0f);

			Vector3 LeftDown = new Vector3(
				_mouseDragStart.x < _mouseDragEnd.x ? _mouseDragStart.x : _mouseDragEnd.x,
				_mouseDragStart.y < _mouseDragEnd.y ? _mouseDragStart.y : _mouseDragEnd.y,
				0.0f);

			_dragUIRectangle.transform.position = ((_mouseDragEnd - _mouseDragStart) * 0.5f);
			_dragUIRectangle.transform.position += (_mouseDragStart);
			SpriteRenderer dragSprite = _dragUIRectangle.GetComponent<SpriteRenderer>();
			if (dragSprite)
			{				
				dragSprite.size = RightTop - LeftDown;
			}

			BoxCollider2D dragBox = _dragUIRectangle.GetComponent<BoxCollider2D>();
			if (dragBox)
			{
				dragBox.size = RightTop - LeftDown;
			}
		}

		if (Input.GetMouseButtonUp(MOUSE_LEFT_CLICK))
			StartCoroutine(OnClickEnd());
	}

	private void OnClickStart()
	{
		if (null == _dragUIRectangle)
			return;

		if (false == _mouseDraging)
		{
			_mouseDragStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_mouseDragStart.z = -Camera.main.transform.position.z;
			//_mouseDragStart += transform.position;
			_dragUIRectangle.SetActive(true);
		}

		_mouseDraging = true;

	}

	private System.Collections.IEnumerator OnClickEnd()
	{
		_mouseDraging = false;

		Drag dragUIRectangle = _dragUIRectangle.GetComponent<Drag>();
		if (null != dragUIRectangle)
		{
			dragUIRectangle.MatchAppleValue();
		}

		yield return null;

		_dragUIRectangle.SetActive(false);
	}

	private void OnMatchScore(int score)
	{
		_appleMatchScore += score;
	}
}
