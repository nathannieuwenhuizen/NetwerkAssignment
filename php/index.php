<?php

include "./connect.php";
include "./globals.php";

//all score below 100
$query = "SELECT * FROM `scores` order by id desc limit 5"; 

//all score from user with socre below 100
//$query = "SELECT * FROM `scores` WHERE user_id = 2 AND value < 100";

//all score of game 3 with score higher than 100
//$query = "SELECT * FROM `scores` WHERE game_id = 3 AND value > 100";

//top 5 scores of game 5
//$query = "SELECT value, user_id FROM `scores` WHERE game_id = 1 ORDER BY value DESC LIMIT 5";

// highscore of player 5 in game 5
//$query = "SELECT * FROM `scores` WHERE game_id = 5 AND user_id = 5 ORDER BY value DESC LIMIT 1";

if (!($result = $mysqli->query($query))) {
    showerror($mysqli->errno,$mysqli->error);
}

$row = $result->fetch_assoc(); 
$rows = array();
do { 
    $rows[] = $row;
} while ($row = $result->fetch_assoc());
echo "Most recent scores: <br>";
echo "{rows:" . json_encode( $rows) . "}" . "<br>";
echo "<br>";

$userID = GetURLVariable("user", 0, -1, 0);
$gameID = GetURLVariable("game", 0, -1, 0); 
$value = GetURLVariable("score", 0, 999, 0);
echo "url variables are: " . $userID . $gameID . $value . "<br>";

if ($userID == 0 || $gameID == 0 || $value == 0) {
    echo "url variable invalid <br>";
} else {
    echo "url variavbles are valid, trying to push it to database... <br>";
    phpinfo();
    //insertScore($mysqli, $userID, $gameID,$value);
}
function insertScore($mysqli, $gameID, $userID, $value) {

    $query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `date`, `value`) VALUES (NULL, '$gameID', '$userID', now(), '$value');";
    if (!($result = $mysqli->query($query))) {
        echo "there is an error... <br>";
        showerror($mysqli->errno,$mysqli->error);
    } else {
        echo "score added! <br>";
    }
}

?>