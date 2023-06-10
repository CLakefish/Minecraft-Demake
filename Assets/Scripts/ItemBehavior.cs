using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBehavior : MonoBehaviour
{
    public int itemID;
    public Vector3 pos;
    public bool firstObj = false;
    Vector3 previousPos;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 50 * Time.deltaTime, Space.Self);

        float changedY = Mathf.Sin(Time.time * (2f)) * .5f + pos.y;
        transform.position = new Vector3(pos.x, changedY, pos.z);

        if (FindObjectsOfType<ItemBehavior>().Length <= 1) firstObj = true;
        if (!firstObj && new Vector3(previousPos.x, 0f, previousPos.z) == new Vector3(transform.position.x, 0f, transform.position.z))
        {
            foreach (ItemBehavior items in FindObjectsOfType<ItemBehavior>())
            {
                if (Vector3.Distance(items.transform.position, transform.position) <= 2) items.transform.position = transform.position;
            }
        }

        previousPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<ChunkInteract>(out ChunkInteract p))
        {
            Debug.Log("Given the player: i" + itemID.ToString());
            Destroy(gameObject);
        }
    }
}
