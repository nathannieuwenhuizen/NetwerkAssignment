<?php

include "./connect.php";
include "./globals.php";

//top 5 scores of game 5
//$query = "SELECT value, user_id FROM `scores` WHERE game_id = 1 ORDER BY value DESC LIMIT 5";
// $query = "SELECT ROUND(AVG(value)) AS gemiddelde, COUNT(scores.id) as amountofwins, u.first_name, u.last_name FROM scores LEFT JOIN users u on (u.id = user_id) GROUP BY user_id ORDER BY gemiddelde DESC";
$query = "SELECT ROUND(AVG(value)) AS gemiddelde, DATE_FORMAT(scores.date, '%m/%d/%Y') as create_date, COUNT(scores.id) as amountofwins, u.first_name, u.last_name FROM scores LEFT JOIN users u on (u.id = user_id) WHERE scores.date > DATE_ADD(NOW(), INTERVAL -1 MONTH) GROUP BY user_id ORDER BY gemiddelde DESC";
if (!($result = $mysqli->query($query))) {
    showerror($mysqli->errno,$mysqli->error);
}

$row = $result->fetch_assoc(); 
$alltimeRows = array();

do { 
    // echo json_encode($row) . "<br>";
    $alltimeRows[] = $row;
} while ($row = $result->fetch_assoc());
//  echo '{"allTime":' . json_encode( $alltimeRows) . '}';
 echo '{"topofmonth":' . json_encode( $alltimeRows) . ' , "test" : "5"}';
// echo "<br>";
 
?> 