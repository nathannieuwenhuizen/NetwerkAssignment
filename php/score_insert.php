<?php

include "./connect.php";
include "./globals.php";

SetExistingSession();
session_start();

if (!CheckServerID()) {
    echo "0"; 
    return;
}

$userID = GetURLVariable("user", -1, -1, 0);
$value = GetURLVariable("score", 0, 9999, 0);
// echo "url variables are: " . $userID .  $value . "<br>";

if ($userID == 0) {
    echo "0";
} else {
    //phpinfo(); 
    insertScore($mysqli, $userID,$value);
}
function insertScore($mysqli, $userID, $value) {

    $query = "INSERT INTO `scores` (`id`, `game_id`, `user_id`, `date`, `value`) VALUES (NULL, '1', '$userID', now(), '$value');";
    if (!($result = $mysqli->query($query))) {
        echo "0"; 
        showerror($mysqli->errno,$mysqli->error);
    } else {
        echo "1"; 
    }
}

?>