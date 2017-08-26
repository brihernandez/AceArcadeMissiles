using UnityEngine;

public class SceneRestarter : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}
	}
}
