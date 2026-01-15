using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Snake : MonoBehaviour
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		_segments.Add(this.transform);
	}

	// Update is called once per frame
	void Update()
	{
		SnakeInput();

		_nextMoveTime += Time.deltaTime;

		if (_nextMoveTime >= _nextMoveTimeInterval)
		{
			_nextMoveTime = 0.0f;
			Move();
		}

	}

	[SerializeField] private Transform bodyPrefab;
	[SerializeField] private const float _nextMoveTimeInterval = 0.75f;
	private float _nextMoveTime = _nextMoveTimeInterval;
	private List<Transform> _segments = new List<Transform>();
	private Vector2 _direction = Vector2.right;
	private Vector3 _lastPosition = Vector3.zero;

	private void SnakeInput()
	{
		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (_direction != Vector2.down)
				_direction = Vector2.up;
		}
		else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (_direction != Vector2.up)
				_direction = Vector2.down;
		}
		else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if (_direction != Vector2.right)
				_direction = Vector2.left;
		}
		else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
		{
			if (_direction != Vector2.left)
				_direction = Vector2.right;
		}
	}

	private void Move()
	{
		_lastPosition = _segments.Last().position;
		
		// 1. 몸통 이동: 뒤에서부터 앞으로 (Reverse Iteration)
		// 꼬리(_segments[i])가 바로 앞 몸통(_segments[i-1])의 위치로 이동
		for (int i = _segments.Count - 1; i > 0; --i)
		{
			_segments[i].position = _segments[i - 1].position;
		}

		// 2. 머리 이동: 현재 위치 + 방향 벡터
		// 지역 변수는 접두사 없이 그냥 camelCase (vNextPos -> nextPos)
		Vector3 nextPos = transform.position + new Vector3(_direction.x, _direction.y, 0);

		// 3. 그리드 스냅 (좌표 보정)
		// 0.9999 좌표 방지용 반올림 처리
		transform.position = new Vector3(
			Mathf.Round(nextPos.x),
			Mathf.Round(nextPos.y),
			0.0f
		);
	}

	private void OnTriggerEnter2D(Collider2D Other)
	{
		if (Other.CompareTag("Food"))
		{
			Grow();
			MoveFoodToRandomPos(Other.gameObject);
		}

		else if (Other.CompareTag("Obstacle"))
		{
			// 게임 오버 처리 (씬 다시 로드)
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}
	}

	private void MoveFoodToRandomPos(GameObject food)
	{
		// 화면 안쪽 적당한 그리드 좌표로 이동 (-8 ~ 8 범위라고 가정)
		// (int)로 캐스팅해서 정수 좌표(그리드)에 딱딱 맞게 해주는 게 포인트
		int x = (int)Random.Range(-8, 8);
		int y = (int)Random.Range(-4, 4);

		food.transform.position = new Vector3(x, y, 0);
	}

	private void Grow()
	{
		Transform segment = Instantiate(bodyPrefab);

		Vector3 spawnPos = Vector3.zero;

		if (_segments.Count == 1)
		{
			spawnPos = _segments[0].position - (Vector3)_direction;
		}
		else
		{
			Transform tail = _segments[_segments.Count - 1];      // 꼬리
			Transform prev = _segments[_segments.Count - 2];      // 꼬리 앞칸

			Vector3 tailDirection = tail.position - prev.position;

			spawnPos = tail.position + tailDirection;
			spawnPos = _lastPosition;
		}

		segment.position = spawnPos;
		_segments.Add(segment);
	}
}
