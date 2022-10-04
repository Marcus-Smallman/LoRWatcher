package main

import (
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
)

func Main(args map[string]interface{}) map[string]interface{} {
	msg := make(map[string]interface{})
	gameName, ok := args["gameName"].(string)
	if !ok {
		msg["body"] = `{"status":{"message":"gameName is required","status_code":400}}`
		return msg
	}

	tagLine, ok := args["tagLine"].(string)
	if !ok {
		msg["body"] = `{"status":{"message":"tagLine is required","status_code":400}}`
		return msg
	}

	client := &http.Client{}
	url := fmt.Sprintf("https://europe.api.riotgames.com/riot/account/v1/accounts/by-riot-id/%s/%s", gameName, tagLine)
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		fmt.Println(err)
		return msg
	}

	apikey := os.Getenv("riotapikey")
	req.Header.Set("X-Riot-Token", apikey)

	resp, err := client.Do(req)
	if err != nil {
		fmt.Println(err)
		return msg
	}
	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		fmt.Println(err)
		return msg
	}

	msg["body"] = string(body)

	return msg
}
