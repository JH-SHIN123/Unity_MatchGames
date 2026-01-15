using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Drag : MonoBehaviour
{
	static private int DRAG_VALUE = 10;

	private List<Apple> _dragAppleList = new List<Apple>();
	private int _currentAppleValue = 0;
	public Action<int> OnAppleMatched;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
	}

    // Update is called once per frame
    void Update()
    {
    }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Apple"))
		{
			Apple apple = other.GetComponent<Apple>();
			if (apple != null)
			{
				_dragAppleList.Add(apple);
				_currentAppleValue += apple.GetAppleValue();

			}
		}

		CheckAppleValue();
	}
	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Apple"))
		{
			Apple apple = other.GetComponent<Apple>();
			if (apple != null)
			{
				apple.SetHighlightAppleValue(false);
				_currentAppleValue -= apple.GetAppleValue();
				_dragAppleList.Remove(apple);

			}
		}
		CheckAppleValue();
	}

	public void MatchAppleValue()
	{
		if (_currentAppleValue == DRAG_VALUE)
		{
			OnAppleMatched?.Invoke(_dragAppleList.Count);

			for (int i = _dragAppleList.Count - 1; i >= 0; i--)
			{
				Apple apple = _dragAppleList[i];

				_dragAppleList.RemoveAt(i);
				if (apple != null)
				{
					Destroy(apple.gameObject);
				}

			}

			_dragAppleList.Clear();
			_currentAppleValue = 0;
		}
	}

	private void CheckAppleValue()
	{
		if (_currentAppleValue == DRAG_VALUE)
		{
			foreach (Apple apple in _dragAppleList)
			{
				apple.SetHighlightAppleValue(true);
			}
		}
		else
		{
			foreach (Apple apple in _dragAppleList)
			{
				apple.SetHighlightAppleValue(false);
			}
		}
	}
}
