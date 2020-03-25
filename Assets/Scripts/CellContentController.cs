using UnityEngine;

namespace DefaultNamespace
{
	public class CellContentController : MonoBehaviour
	{
		public MeshRenderer colorRenderer;
		public MeshRenderer colorRendererSpecial;
		public GameObject Frame;
		
		public void SetContent(CellContent cellContent)
		{
			MeshRenderer renderer;
			if (cellContent.type == CellType.Special)
			{
				renderer = colorRendererSpecial;
				colorRenderer.gameObject.SetActive(false);
			}
			else
			{
				renderer = colorRenderer;
				colorRendererSpecial.gameObject.SetActive(false);
			}
			renderer.gameObject.SetActive(true);
			var propBlock = new MaterialPropertyBlock();
			propBlock.SetColor("_Color", cellContent.Color);
			renderer.SetPropertyBlock(propBlock); 
		}

		public void SetSelected(bool value)
		{
			Frame.SetActive(value);
		}
	}
}