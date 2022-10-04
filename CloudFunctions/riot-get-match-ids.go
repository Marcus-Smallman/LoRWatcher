package main

import (
	"fmt"
	"io/ioutil"
	"net/http"
	"os"
)

func Main(args map[string]interface{}) map[string]interface{} {
	msg := make(map[string]interface{})
	playerId, ok := args["playerId"].(string)
	if !ok {
		msg["body"] = `{"status":{"message":"playerId is required","status_code":400}}`
		return msg
	}

	region, ok := args["region"].(string)
	if !ok {
		region = "europe"
	}

	client := &http.Client{}
	url := fmt.Sprintf("https://%s.api.riotgames.com/lor/match/v1/matches/by-puuid/%s/ids", region, playerId)
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
