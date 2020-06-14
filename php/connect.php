<?php

$db_user = "root";
$db_pass = "";
$db_host = "localhost";
$db_name = "kernmodule4db";


/* Open a connection */
$mysqli = new mysqli($db_host,$db_user,$db_pass,$db_name);

/* check connection */
if ($mysqli->connect_errno) {
   echo "Failed to connect to MySQL: (" . $mysqli->connect_errno() . ") " . $mysqli->connect_error();
   exit();
} else {
    //echo "Connection succesful! <br><br>";
}

function showerror($error,$errornr) {
    die("Error (" . $errornr . ") " . $error);
    }    
?>
