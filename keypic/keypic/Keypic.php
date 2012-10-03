<?php

ini_set('display_errors', 'On');
error_reporting(E_ALL & ~E_NOTICE);

/*  Copyright 2010-2012  Keypic LLC (email : info@keypic.com)

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License, version 2, as 
    published by the Free Software Foundation.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

	
	Works only with PHP >= 5.3.0
*/

class Keypic
{
	private static $Instance;
	private static $version = '1.3';
	private static $UserAgent = 'User-Agent: Keypic PHP5 Class, Version: 1.3';
	private static $host = 'ws.keypic.com';
	private static $url = '/';
	private static $port = 80;

	private static $FormID;
	private static $Token;
	private static $RequestType;
	private static $WeightHeight;
	private static $Debug;

	private function __clone(){}

	public function __construct(){}

	public static function getInstance()
	{
		if (!self::$Instance)
		{
			self::$Instance = new self();
		}

		return self::$Instance;
	}

	public static function setVersion($version)
	{
		self::$version = $version;
	}

	public static function setUserAgent($UserAgent)
	{
		self::$UserAgent = $UserAgent;
	}

	public static function setFormID($FormID)
	{
		self::$FormID = $FormID;
	}

	public static function checkFormID($FormID)
	{
		$fields['RequestType'] = 'checkFormID';
		$fields['ResponseType'] = '2';
		$fields['FormID'] = $FormID;

		$response = json_decode(self::sendRequest($fields), true);
		return $response;
	}

	public static function setDebug($Debug)
	{
		self::$Debug = $Debug;
	}

	// makes a request to the Keypic Web Service
	private static function sendRequest($fields)
	{
		// boundary generation
		srand((double)microtime()*1000000);
		$boundary = "---------------------".substr(md5(rand(0,32000)),0,10);

		// Build the header
		$header = "POST " . self::$url . " HTTP/1.0\r\n";
		$header .= "Host: " . self::$host . "\r\n";
		$header .= "Content-type: multipart/form-data, boundary=$boundary\r\n";
		$header .= self::$UserAgent . "\r\n";

		// attach post vars
		foreach($fields AS $index => $value)
		{
			$data .="--$boundary\r\n";
			$data .= "Content-Disposition: form-data; name=\"$index\"\r\n";
			$data .= "\r\n$value\r\n";
			$data .="--$boundary\r\n";
		}

		// and attach the file
//		$data .= "--$boundary\r\n";
//		$content_file = join("", file($tmp_name));
//		$data .="Content-Disposition: form-data; name=\"userfile\"; filename=\"$file_name\"\r\n";
//		$data .= "Content-Type: $content_type\r\n\r\n";
//		$data .= "$content_file\r\n";
//		$data .="--$boundary--\r\n";

		$header .= "Content-length: " . strlen($data) . "\r\n\r\n";

		$socket = new Socket(self::$host, self::$port, $header.$data);
		$socket->send();
		$return = explode("\r\n\r\n", $socket->getResponse(), 2);
		return $return[1];
	}

	public static function getToken($Token, $ClientEmailAddress = '', $ClientUsername = '', $ClientMessage = '', $ClientFingerprint = '', $Quantity = 1)
	{
		if($Token)
		{
			self::$Token = $Token;
			return self::$Token;
		}
		else
		{

			$fields['FormID'] = self::$FormID;
			$fields['RequestType'] = 'RequestNewToken'; // 001
			$fields['ResponseType'] = '2';
			$fields['ServerName'] = $_SERVER['SERVER_NAME'];
			$fields['Quantity'] = $Quantity;
			$fields['ClientIP'] = $_SERVER['REMOTE_ADDR'];
			$fields['ClientUserAgent'] = $_SERVER['HTTP_USER_AGENT'];
			$fields['ClientAccept'] = $_SERVER['HTTP_ACCEPT'];
			$fields['ClientAcceptEncoding'] = $_SERVER['HTTP_ACCEPT_ENCODING'];
			$fields['ClientAcceptLanguage'] = $_SERVER['HTTP_ACCEPT_LANGUAGE'];
			$fields['ClientAcceptCharset'] = $_SERVER['HTTP_ACCEPT_CHARSET'];
			$fields['ClientHttpReferer'] = $_SERVER['HTTP_REFERER'];
			$fields['ClientUsername'] = $ClientUsername;
			$fields['ClientEmailAddress'] = $ClientEmailAddress;
			$fields['ClientMessage'] = $ClientMessage;
			$fields['ClientFingerprint'] = $ClientFingerprint;

			$response = json_decode(self::sendRequest($fields), true);

			if($response['status'] == 'new_token')
			{
				self::$Token = $response['Token'];
				return  $response['Token'];
			}
		}
	}

	public static function getImage($WeightHeight = null, $Debug = null)
	{
		return '<a href="http://' . self::$host . '/?RequestType=getClick&amp;Token=' . self::$Token . '" target="_blank"><img src="http://' . self::$host . '/?RequestType=getImage&amp;Token=' . self::$Token . '&amp;WeightHeight=' . $WeightHeight . '&amp;Debug=' . self::$Debug . '" alt="Form protected by Keypic" /></a>';
	}

	public static function getiFrame($WeightHeight = null)
	{
		if($WeightHeight)
		{
			$xy = explode('x', $WeightHeight);
			$x = (int)$xy[0];
			$y = (int)$xy[1];
		}
		else{$x=88; $y=31;}

		$url = 'http://' . self::$host . '/?RequestType=getiFrame&amp;WeightHeight=' . $WeightHeight . '&amp;Token=' . self::$Token;


	return <<<EOT
<iframe
src="$url"
width="$x"
height="$y"
frameborder="0"
style="border: 1px solid #ffffff; background-color: #ffffff;"
marginwidth="0"
marginheight="0"
vspace="0"
hspace="0"
allowtransparency="true"
scrolling="no"><p>Your browser does not support iframes.</p></iframe>
EOT;
	}

	public static function isSpam($Token, $ClientEmailAddress = '', $ClientUsername = '', $ClientMessage = '', $ClientFingerprint = '')
	{

		self::$Token = $Token;
		$fields['Token'] = self::$Token;
		$fields['FormID'] = self::$FormID;
		$fields['RequestType'] = 'RequestValidation'; // 002
		$fields['ResponseType'] = '2';
		$fields['ServerName'] = $_SERVER['SERVER_NAME'];
		$fields['ClientIP'] = $_SERVER['REMOTE_ADDR'];
		$fields['ClientUserAgent'] = $_SERVER['HTTP_USER_AGENT'];
		$fields['ClientAccept'] = $_SERVER['HTTP_ACCEPT'];
		$fields['ClientAcceptEncoding'] = $_SERVER['HTTP_ACCEPT_ENCODING'];
		$fields['ClientAcceptLanguage'] = $_SERVER['HTTP_ACCEPT_LANGUAGE'];
		$fields['ClientAcceptCharset'] = $_SERVER['HTTP_ACCEPT_CHARSET'];
		$fields['ClientHttpReferer'] = $_SERVER['HTTP_REFERER'];
		$fields['ClientUsername'] = $ClientUsername;
		$fields['ClientEmailAddress'] = $ClientEmailAddress;
		$fields['ClientMessage'] = $ClientMessage;
		$fields['ClientFingerprint'] = $ClientFingerprint;

		$response = json_decode(self::sendRequest($fields), true);

		if($response['status'] == 'response'){return $response['spam'];}
		else if($response['status'] == 'error'){return $response['error'];}
	}

	public static function reportSpam($Token)
	{
		if($Token == ''){return 'error';}
		if(self::$FormID == ''){return 'error';}

		$fields['Token'] = $Token;
		$fields['FormID'] = self::$FormID;
		$fields['RequestType'] = 'ReportSpam';
		$fields['ResponseType'] = '2';

		$response = json_decode(self::sendRequest($fields), true);
		return $response;
	}

}

class Socket
{
	private $host;
	private $port;
	private $request;
	private $response;
	private $responseLength;
	private $errorNumber;
	private $errorString;
	private $timeout;
	private $retry;

	public function __construct($host, $port, $request, $responseLength = 1024, $timeout = 3, $retry = 3)
	{
		$this->host = $host;
		$this->port = $port;
		$this->request = $request;
		$this->responseLength = $responseLength;
		$this->errorNumber = 0;
		$this->errorString = '';
		$this->timeout = $timeout;
		$this->retry = $retry;
	}

	public function Send()
	{
		$this->response = '';
		$r = 0;

		do
		{
			if($r >= $this->retry){return;}

			$fs = fsockopen($this->host, $this->port, $this->errorNumber, $this->errorString, $this->timeout);
			++$r;
		}
		while(!$fs);

		if($this->errorNumber != 0){throw new Exception('Error connecting to host: ' . $this->host . ' Error number: ' . $this->errorNumber . ' Error message: ' . $this->errorString);}

		if($fs !== false)
		{
			@fwrite($fs, $this->request);

			while(!feof($fs))
			{
				$this->response .= fgets($fs, $this->responseLength);
			}

			fclose($fs);
			
		}
	}

	public function getResponse(){return $this->response;}

	public function getErrorNumner(){return $this->errorNumber;}

	public function getErrorString(){return $this->errorString;}
}
