# EnemyCoverSystem
###### Enemy:<br>
- Patrols Between Nodes.<br>
- If cames close with a fellow enemy then they chat for a few secs.<br>
- If cames contact with the player then based on the distance to the player and any (safe-free)cover in range it decides what to do.<br>
- If Player is closer than the cover, enemy decides to run towards Player and starts shooting.<br>
- If the cover  is closer than the enemy and is safe(no player vision) and free(not occupied by other fellow enemies) then enemy moves to that cover.<br>


![EnemyCoverSystem5](https://user-images.githubusercontent.com/29523816/66151853-d053b700-e620-11e9-81db-1eff7aae5113.gif)



###### Enemy In Cover:<br>
- Attacks to Player periodically.<br>
- If Player moves to a position where it can see the enemy which makes the cover unsafe for enemy then enemy moves to a safe closest cover, if there aren't any safe cover enemy fires on spot.<br>



![EnemyCoverSystem](https://user-images.githubusercontent.com/29523816/66149219-43f2c580-e61b-11e9-98ec-133df772fbaf.gif)

![EnemyCoverSystem3](https://user-images.githubusercontent.com/29523816/66150603-3854ce00-e61e-11e9-99a5-28cc8444412e.gif)


