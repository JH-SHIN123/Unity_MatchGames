using TMPro;
using UnityEngine;

public class Apple : MonoBehaviour
{
	[SerializeField] private Color _defaultColor;
	[SerializeField] private Color _hightlightColor;

	private int _appleValue = -1;
	private GameObject _spawnValueUI;
	public int GetAppleValue() {return _appleValue;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

	}

	// Update is called once per frame
	void Update()
    {
        
    }

	public void Initialize(int appleValue, GameObject valueUI)
	{
		_appleValue = appleValue;

		if (null != valueUI)
		{
			_spawnValueUI = Instantiate(valueUI, transform.position, Quaternion.identity);
			_spawnValueUI.transform.SetParent(this.transform, true);

			TextMeshPro textComponent = _spawnValueUI.GetComponent<TextMeshPro>();
			if (null != textComponent)
			{
				textComponent.text = _appleValue.ToString();
				textComponent.color = _defaultColor;
			}
		}
	}

	public void SetHighlightAppleValue(bool isHightlight)
	{
		TextMeshPro textComponent = _spawnValueUI.GetComponent<TextMeshPro>();
		if (null != textComponent)
		{
			textComponent.color = isHightlight ? _hightlightColor : _defaultColor;
		}
	}
}
