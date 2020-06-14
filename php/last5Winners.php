<?php

include "./connect.php";
include "./globals.php";

//top 5 scores of game 5
//$query = "SELECT value, user_id FROM `scores` WHERE game_id = 1 ORDER BY value DESC LIMIT 5";
$query = "SELECT s.date, s.value, u.first_name, u.last_name FROM scores s LEFT JOIN users u ON(s.user_id = u.id) ORDER BY s.date DESC LIMIT 5";

if (!($result = $mysqli->query($query))) {
    showerror($mysqli->errno,$mysqli->error);
}

$row = $result->fetch_assoc(); 
$rows = array();
echo "Most recent scores: <br>";

do { 
    echo json_encode($row) . "<br>";
    $rows[] = $row;
} while ($row = $result->fetch_assoc());
// echo "{rows:" . json_encode( $rows) . "}" . "<br>";
// echo "<br>";

?> 