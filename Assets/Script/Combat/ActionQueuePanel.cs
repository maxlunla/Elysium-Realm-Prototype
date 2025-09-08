using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionQueuePanel : MonoBehaviour
{
	[Header("Action Icons")]
	public Image[] actionIcons; // 5 icons

	[Header("AP/Action Texts")]
	public TMP_Text tpText;
	public TMP_Text actionText;

	[Header("Execute Controls Panel")]
	public GameObject executeControlsPanel;
}