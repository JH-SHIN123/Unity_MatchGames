using System;
using System.Collections.Specialized;
using UnityEngine;


public class MatchGameRule : MonoBehaviour
{
	const int MOUSE_LEFT_CLICK = 0;

	[Header("Grid Settings")]
	[SerializeField] private int width = 6;   // 가로 크기
	[SerializeField] private int height = 8;  // 세로 크기
	[SerializeField] private float cellSize = 1.0f; // 보석 간격

	[Header("References")]
	[SerializeField] private GameObject gemPrefab; // 보석 껍데기 프리팹
	[SerializeField] private GemData[] gemTypes;   // 보석 데이터 목록 (Red, Blue, Green...)

	[Header("Input")]
	private Gem _firstSelectedGem = null; // 첫 번째로 선택한 보석
	private bool _isProcessing = false;   // 애니메이션/매칭 중인지 (입력 막기용)

	[Header("Data")]
	[SerializeField] private Gem[] _allGems;
	[SerializeField] private System.Collections.Generic.HashSet<Gem> _distroyGems = new System.Collections.Generic.HashSet<Gem>();

	private void Start()
	{
		_allGems = new Gem[width * height];
		GenerateGrid();
	}

	private void Update()
	{
		if (_isProcessing) 
			return;

		if (Input.GetMouseButtonDown(MOUSE_LEFT_CLICK))
		{
			SelectGem();
		}
	}

	private void GenerateGrid()
	{
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				int currentIndex = y * width + x;
				int randomIndex = 0;
				bool isMatch = true;

				while (isMatch)
				{
					isMatch = false;
					randomIndex = UnityEngine.Random.Range(0, gemTypes.Length);
					GemData candidateData = gemTypes[randomIndex]; 

					// 1. 왼쪽 검사 (x가 2 이상일 때만)
					if (x >= 2)
					{
						GemData left1 = _allGems[currentIndex - 1].GetComponent<Gem>().data;
						GemData left2 = _allGems[currentIndex - 2].GetComponent<Gem>().data;

						if (left1 == candidateData && left2 == candidateData)
						{
							isMatch = true; // 다시 뽑아야 함
						}
					}

					if (y >= 2)
					{
						GemData down1 = _allGems[currentIndex - width].GetComponent<Gem>().data;
						GemData down2 = _allGems[currentIndex - (width * 2)].GetComponent<Gem>().data;

						if (down1 == candidateData && down2 == candidateData)
						{
							isMatch = true; // 다시 뽑아야 함
						}
					}
				}

				GemData finalData = gemTypes[randomIndex];
				Vector3 spawnPos = new Vector3(x, y, 0) * cellSize;

				GameObject newGem = Instantiate(gemPrefab, spawnPos, Quaternion.identity);

				newGem.transform.parent = this.transform;
				newGem.name = $"Gem [{x},{y}]";

				// 데이터 주입
				Gem gemComponent = newGem.GetComponent<Gem>();
				if (gemComponent != null)
				{
					gemComponent.Initialize(finalData, y * width + x);

				}

				// 배열에 저장
				_allGems[currentIndex] = gemComponent;
			}
		}

		CenterCamera();
	}
	private void CenterCamera()
	{
		Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
	}

	private void SelectGem()
	{
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

		if (hit.collider != null)
		{
			Gem clickedGem = hit.collider.GetComponent<Gem>();
			if (clickedGem != null)
			{
				// A. 첫 번째 선택
				if (_firstSelectedGem == null)
				{
					_firstSelectedGem = clickedGem;
					HighlightGem(_firstSelectedGem, true);
				}
				// B. 두 번째 선택 (교체 시도)
				else
				{
					// 같은 걸 또 누르면 취소
					if (_firstSelectedGem == clickedGem)
					{
						HighlightGem(_firstSelectedGem, false);
						_firstSelectedGem = null;
					}
					else
					{
						HighlightGem(_firstSelectedGem, false);

						// 두 보석의 거리가 1.5 이하일 때만 스왑 (대각선 제외, 인접한 것만)
						if (Vector2.Distance(_firstSelectedGem.transform.position, clickedGem.transform.position) <= 1.2f)
						{
							StartCoroutine(SwapGem(_firstSelectedGem, clickedGem));
						}
						else
						{
							// 너무 멀면 그냥 새로 선택한 걸로 갱신
							_firstSelectedGem = clickedGem;
							HighlightGem(_firstSelectedGem, true);
						}
					}
				}
			}
		}
	}

	// 선택된 보석의 색을 살짝 어둡게 하거나 크기를 키우는 함수
	private void HighlightGem(Gem gem, bool isSelected)
	{
		SpriteRenderer sr = gem.GetComponent<SpriteRenderer>();

		sr.color = isSelected ? Color.gray : gem.data.color;
	}

	private System.Collections.IEnumerator SwapGem(Gem gemA, Gem gemB)
	{
		_isProcessing = true;
		_firstSelectedGem = null;

		Vector3 posA = gemA.transform.position;
		Vector3 posB = gemB.transform.position;

		float elapsed = 0f;
		float duration = 0.3f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;

			// Lerp로 부드럽게 이동
			gemA.transform.position = Vector3.Lerp(posA, posB, t);
			gemB.transform.position = Vector3.Lerp(posB, posA, t);

			yield return null;
		}

		// 확실하게 위치 고정
		gemA.transform.position = posB;
		gemB.transform.position = posA;

		yield return new WaitForSeconds(0.25f);

		CheckGemMatch(gemA, gemB);
	}

	private bool CheckGemMatch(Gem gemA, Gem gemB)
	{
		int indexA = gemA.GetGemGrid();
		int indexB = gemB.GetGemGrid();

		gemA.SetGemGrid(indexB);
		gemB.SetGemGrid(indexA);

		Gem tempObj = _allGems[indexA];
		_allGems[indexA] = _allGems[indexB];
		_allGems[indexB] = tempObj;

		CheckAndProcessMatches();

		return false;
	}

	private bool CheckAndProcessMatches()
	{
		for (int i = 0; i < width * height; i++)
		{
			int x = i % width;
			int y = i / width;

			// 1. 가로 검사 (오른쪽으로 2개 더 확인)
			if (x < width - 2)
			{
				Gem g1 = _allGems[i];
				Gem g2 = _allGems[i + 1];
				Gem g3 = _allGems[i + 2];

				if (IsMatching(g1, g2, g3))
				{
					_distroyGems.Add(g1);
					_distroyGems.Add(g2);
					_distroyGems.Add(g3);
				}
			}

			// 2. 세로 검사 (위쪽으로 2개 더 확인)
			if (y < height - 2)
			{
				Gem g1 = _allGems[i];
				Gem g2 = _allGems[i + width];
				Gem g3 = _allGems[i + (width * 2)];

				if (IsMatching(g1, g2, g3))
				{
					_distroyGems.Add(g1);
					_distroyGems.Add(g2);
					_distroyGems.Add(g3);
				}
			}
		}

		if (_distroyGems.Count > 0)
		{
			StartCoroutine(DestroyGem());

			return true;
		}

		return false;
	}

	private bool IsMatching(Gem a, Gem b, Gem c)
	{
		if (a == null || b == null || c == null) 
			return false;

		return (a.data.color == b.data.color) && (a.data.color == c.data.color);
	}

	private System.Collections.IEnumerator DestroyGem()
	{
		yield return new WaitForSeconds(0.25f);
		
		foreach (Gem gem in _distroyGems)
		{
			if (gem != null)
			{
				_allGems[gem.GetGemGrid()] = null; // 배열에서 참조 제거

				Destroy(gem.gameObject);
				Debug.Log("Destroy");
			}
		}
		yield return new WaitForSeconds(0.1f);
		yield return StartCoroutine(ApplyGravity());
	}

	private System.Collections.IEnumerator ApplyGravity()
	{
		int gravityCount = 0;

		for (int x = 0; x < width ; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				int index = y * width + x;

				if (null == _allGems[index])
				{
					int increase = y;
					for (int nextY = y + 1; nextY < height; nextY++)
					{
						int upIndex = nextY * width + x;

						if (null == _allGems[upIndex])
							continue;

						_allGems[index] = _allGems[upIndex];
						_allGems[upIndex] = null;

						Vector3 newPos = Vector3.zero;
						newPos.x = x;
						newPos.y = y;
						newPos *= cellSize;

						_allGems[index].MoveGem(newPos);
						_allGems[index].SetGemGrid(index);
						++gravityCount;
						break;
					}
				}
			}
			yield return null;
		}



		_distroyGems.Clear();
		yield return new WaitForSeconds(0.2f);

		StartCoroutine(RefillGrid());

		//yield return new WaitForSeconds((gravityCount/3) * 0.4f);
		yield return new WaitForSeconds(1f);

		_isProcessing = CheckAndProcessMatches();

		if(_isProcessing)
		{

		}
		else
		{
			CheckAndProcessMatches();
		}
	}

	private System.Collections.IEnumerator RefillGrid()
	{
		for (int x = 0; x < width; x++)
		{
			int missingCount = 0;

			for (int y = 0; y < height; y++)
			{
				int index = y * width + x;

				if (_allGems[index] == null)
				{
					++missingCount;

					Vector3 targetPos = new Vector3(x, y + missingCount, 0.0f) * cellSize;
					_allGems[index] = CreateRandomGem(targetPos, index);

					if(_allGems[index])
					{
						targetPos.y -= missingCount;	
						_allGems[index].MoveGem(targetPos);
					}
				}
			}
		}

		// 모든 보석이 떨어지는 시간을 고려해 대기
		yield return new WaitForSeconds(0.5f);
	}

	private Gem CreateRandomGem(Vector3 spawnPos, int index)
	{
		GemData newData = gemTypes[UnityEngine.Random.Range(0, gemTypes.Length)];
		GameObject newGemObj = Instantiate(gemPrefab, spawnPos, Quaternion.identity);
		newGemObj.transform.parent = this.transform;

		Gem newGem = newGemObj.GetComponent<Gem>();
		newGem.Initialize(newData, index);

		return newGem;
	}

	//private System.Collections.IEnumerator ProcessMatchChain()
	//{
	//	while (true) // 연쇄 반응(콤보)을 위해 루프 사용
	//	{
	//		// 1. 매치된 블록 찾기
	//		var matches = FindMatches();
	//		if (matches.Count == 0) 
	//			break;

	//		// 2. 블록 파괴
	//		foreach (var gemObj in matches)
	//		{
	//			if (gemObj != null)
	//			{
	//				Gem gem = gemObj.GetComponent<Gem>();
	//				_allGems[gem.GetGemGrid()] = null; // 배열에서 참조 제거
	//				Destroy(gemObj);
	//			}
	//		}

	//		yield return new WaitForSeconds(0.2f);

	//		// 3. 중력 적용 (블록 내리기)
	//		yield return StartCoroutine(ApplyGravity());

	//		// 4. 빈칸 채우기
	//		yield return StartCoroutine(RefillGrid());
	//	}
	//}
}