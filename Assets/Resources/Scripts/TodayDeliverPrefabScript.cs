using UnityEngine;

public class TodayDeliverPrefabScript : MonoBehaviour
{
    public Post post;
    public PostManager postManager;

    public void Select()
    {
        if (postManager.SelectedPost == post)
        {
            postManager.SelectedPost = null;
        }
        else
        {
            postManager.SelectedPost = post;
        }

        postManager.RenderSelectedDeliver();
    }
}
