using System.Collections;
using System.Collections.Generic; using UnityEngine.UI;
using UnityEngine;

public class ChunkInteract : MonoBehaviour
{
    [SerializeField] public LayerMask ChunkMask;
    [SerializeField] public LayerMask BoundsMask;

    [Header("Player Values")]
    [SerializeField] private Transform playerCam;
    [SerializeField] float interactableRange;
    [SerializeField] GameObject visualObj;
    [SerializeField] GameObject droppedObj;
    [SerializeField] GameObject special;
    [SerializeField] internal int placableObject;
    int selectedIndex;
    GameObject visual;
    Vector3Int previousBlock;
    private WorldGenerator worldGen;

    [Header("Canvas")]
    [SerializeField] GameObject HotbarPanel;
    [SerializeField] List<Image> hotbarItems;
    internal bool canInteract = true;

    // Start is called before the first frame update
    void Start()
    {
        worldGen = FindObjectOfType<WorldGenerator>();

        selectedIndex = 1;
        placableObject = selectedIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canInteract) return;

        Debug.DrawRay(transform.position, playerCam.forward);

        Ray camRay = new Ray(playerCam.position, playerCam.forward);

        UpdateSelectedIndex();

        if (Physics.Raycast(camRay, out RaycastHit obj, interactableRange, ChunkMask))
        {
            Vector3 targetPoint = obj.point - obj.normal * 0.1f;

            Vector3Int targettedBlock = new Vector3Int
            {
                x = Mathf.RoundToInt(targetPoint.x),
                y = Mathf.RoundToInt(targetPoint.y),
                z = Mathf.RoundToInt(targetPoint.z)
            };

            if (visual != null) Destroy(visual);

            visual = Instantiate(visualObj, targettedBlock, Quaternion.identity);

            //previousBlock = targettedBlock;

            if (Input.GetMouseButtonDown(0))
            {
                if (obj.collider.gameObject.tag == "Finish")
                {
                    Destroy(obj.collider.gameObject);
                }

                string chunkName = obj.collider.gameObject.name;

                if (chunkName.Contains("Chunk"))
                {
                    worldGen.SetBlock(targettedBlock, 0, true);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (obj.collider.gameObject.tag == "Finish")
                {
                    if (obj.collider.transform.TryGetComponent<IInteractObj>(out IInteractObj current))
                    {
                        current.onInteract();
                    }
                    else if (obj.collider.transform.parent.TryGetComponent<IInteractObj>(out IInteractObj c))
                    {
                        c.onInteract();
                    }
                }

                targetPoint = obj.point + obj.normal * 0.1f;

                targettedBlock = new Vector3Int
                {
                    x = Mathf.RoundToInt(targetPoint.x),
                    y = Mathf.RoundToInt(targetPoint.y),
                    z = Mathf.RoundToInt(targetPoint.z)
                };

                if (!Physics.CheckBox(targettedBlock, Vector3.one * .5f, Quaternion.identity, BoundsMask))
                {
                    string chunkName = obj.collider.gameObject.name;

                    if (chunkName.Contains("Chunk"))
                    {
                        worldGen.SetBlock(targettedBlock, placableObject, false, (placableObject == 0) ? special : null);
                    }
                }
            }
        }
        else if (visual != null) Destroy(visual);
    }

    void UpdateSelectedIndex()
    {
        hotbarItems[selectedIndex].color = new Color(1, 1, 1, .5f);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (selectedIndex + 1 >= hotbarItems.Count) selectedIndex = 0;
            else selectedIndex++;
        }

        hotbarItems[selectedIndex].color = new Color(1, 1, 1, .8f);
        placableObject = selectedIndex;
    }
}
