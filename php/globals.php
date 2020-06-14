<?php 

function GetURLVariable($urlVar,$minNumber, $maxNumber, $defaultVal = 0) {
    $result = $defaultVal;
    if ( isset($_GET[$urlVar]) || !empty($_GET[$urlVar]))
    {
        $result = $_GET[$urlVar];
    }
    if ($result > $maxNumber && $maxNumber != -1) {
        $offresultset = $maxNumber;
        }
    if ($result < 0 && $minNumber != -1) {
        $result = $minNumber;
    }

    return $result;
}

function SetExistingSession() {
    if (isset($_GET['session_id'])) { 
        $sid=htmlspecialchars($_GET['session_id']);
        session_id($sid);
    }
}

function CheckServerID() {
    if (isset($_SESSION["server_id"]) && $_SESSION["server_id"]!=0) {
        return true;
    } else {
        return false;
    }
}

?>
