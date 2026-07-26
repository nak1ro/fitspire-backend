# Frontend API cleanup handoff

No file under `fitspire-web` was modified during backend cleanup. New UI work must use the canonical endpoints below and must not use the removed compatibility aliases.

| Removed backend route | Canonical replacement | Frontend cleanup target |
| --- | --- | --- |
| `POST /api/workout/{id}/complete` | `POST /api/workout/{id}/finish` | Remove `complete` from `src/features/workout/api/routes.ts`; ensure `completeWorkout` in `src/features/workout/api/client.ts` and its mutation use `finish`. |
| `POST /api/social/posts/{postId}/like` | `POST` / `DELETE /api/social/posts/{postId}/likes` | Remove `like` from `src/features/social/api/routes.ts`, plus deprecated `togglePostLike` and `useTogglePostLike`. |
| `POST /api/social/posts/{postId}/comments/{commentId}/like` | `POST` / `DELETE /api/social/posts/{postId}/comments/{commentId}/likes` | Remove `commentLike`, deprecated `toggleCommentLike`, and `useToggleCommentLike`. |
| `POST /api/social/follow/{targetUserId}` | `POST /api/social/users/{userId}/follow`; `DELETE` on the same route to unfollow | Remove `follow`, deprecated `toggleFollowUser`, and `useToggleFollowUser`. |

The canonical `likePost`, `unlikePost`, `likeComment`, `unlikeComment`, `followUser`, and `unfollowUser` client functions are already present. The media upload `complete` route is unrelated and must remain.
