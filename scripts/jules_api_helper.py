#!/usr/bin/env python3
"""
Jules API Helper Script

This script provides utilities for interacting with the Google Jules API.
It can list sources, create sessions, monitor sessions, and list activities.
"""

import argparse
import json
import os
import sys
import time
from typing import Dict, List, Optional
import urllib.request
import urllib.error


class JulesAPIClient:
    """Client for interacting with Google Jules API"""
    
    BASE_URL = "https://jules.googleapis.com/v1alpha"
    
    def __init__(self, api_key: str):
        """
        Initialize Jules API client
        
        Args:
            api_key: Jules API key for authentication
        """
        if not api_key:
            raise ValueError("API key is required")
        self.api_key = api_key
    
    def _make_request(self, endpoint: str, method: str = "GET", data: Optional[Dict] = None) -> Dict:
        """
        Make HTTP request to Jules API
        
        Args:
            endpoint: API endpoint (without base URL)
            method: HTTP method (GET or POST)
            data: Optional request body for POST requests
            
        Returns:
            API response as dictionary
        """
        url = f"{self.BASE_URL}/{endpoint}"
        headers = {
            "X-Goog-Api-Key": self.api_key,
            "Content-Type": "application/json"
        }
        
        req_data = json.dumps(data).encode('utf-8') if data else None
        request = urllib.request.Request(url, data=req_data, headers=headers, method=method)
        
        try:
            with urllib.request.urlopen(request) as response:
                return json.loads(response.read().decode('utf-8'))
        except urllib.error.HTTPError as e:
            error_body = e.read().decode('utf-8')
            print(f"Error: HTTP {e.code} - {e.reason}", file=sys.stderr)
            print(f"Response: {error_body}", file=sys.stderr)
            raise
    
    def list_sources(self, page_size: int = 10, page_token: Optional[str] = None) -> Dict:
        """
        List available sources
        
        Args:
            page_size: Number of sources per page
            page_token: Token for pagination
            
        Returns:
            List of sources
        """
        endpoint = f"sources?pageSize={page_size}"
        if page_token:
            endpoint += f"&pageToken={page_token}"
        
        return self._make_request(endpoint)
    
    def get_source(self, owner: str, repo: str) -> Optional[str]:
        """
        Get source name for a GitHub repository
        
        Args:
            owner: Repository owner
            repo: Repository name
            
        Returns:
            Source name or None if not found
        """
        sources = self.list_sources()
        for source in sources.get("sources", []):
            github_repo = source.get("githubRepo", {})
            if github_repo.get("owner") == owner and github_repo.get("repo") == repo:
                return source.get("name")
        return None
    
    def create_session(
        self,
        prompt: str,
        source: str,
        title: str,
        starting_branch: str = "main",
        automation_mode: str = "AUTO_CREATE_PR",
        require_plan_approval: bool = False
    ) -> Dict:
        """
        Create a new Jules session
        
        Args:
            prompt: Task prompt/description
            source: Source name (e.g., "sources/github/owner/repo")
            title: Session title
            starting_branch: Git branch to start from
            automation_mode: AUTO_CREATE_PR, MANUAL, or NONE
            require_plan_approval: Whether to require explicit plan approval
            
        Returns:
            Session details
        """
        data = {
            "prompt": prompt,
            "sourceContext": {
                "source": source,
                "githubRepoContext": {
                    "startingBranch": starting_branch
                }
            },
            "automationMode": automation_mode,
            "title": title,
            "requirePlanApproval": require_plan_approval
        }
        
        return self._make_request("sessions", method="POST", data=data)
    
    def get_session(self, session_id: str) -> Dict:
        """
        Get session details
        
        Args:
            session_id: Session ID
            
        Returns:
            Session details
        """
        return self._make_request(f"sessions/{session_id}")
    
    def list_sessions(self, page_size: int = 10, page_token: Optional[str] = None) -> Dict:
        """
        List all sessions
        
        Args:
            page_size: Number of sessions per page
            page_token: Token for pagination
            
        Returns:
            List of sessions
        """
        endpoint = f"sessions?pageSize={page_size}"
        if page_token:
            endpoint += f"&pageToken={page_token}"
        
        return self._make_request(endpoint)
    
    def approve_plan(self, session_id: str) -> Dict:
        """
        Approve the latest plan for a session
        
        Args:
            session_id: Session ID
            
        Returns:
            Response from API
        """
        return self._make_request(f"sessions/{session_id}:approvePlan", method="POST")
    
    def send_message(self, session_id: str, prompt: str) -> Dict:
        """
        Send a message to the agent
        
        Args:
            session_id: Session ID
            prompt: Message to send
            
        Returns:
            Response from API
        """
        data = {"prompt": prompt}
        return self._make_request(f"sessions/{session_id}:sendMessage", method="POST", data=data)
    
    def list_activities(
        self,
        session_id: str,
        page_size: int = 30,
        page_token: Optional[str] = None
    ) -> Dict:
        """
        List activities in a session
        
        Args:
            session_id: Session ID
            page_size: Number of activities per page
            page_token: Token for pagination
            
        Returns:
            List of activities
        """
        endpoint = f"sessions/{session_id}/activities?pageSize={page_size}"
        if page_token:
            endpoint += f"&pageToken={page_token}"
        
        return self._make_request(endpoint)
    
    def monitor_session(self, session_id: str, interval: int = 30, timeout: int = 3600) -> Dict:
        """
        Monitor a session until it completes or timeout
        
        Args:
            session_id: Session ID
            interval: Check interval in seconds
            timeout: Maximum time to wait in seconds
            
        Returns:
            Final session details
        """
        start_time = time.time()
        
        while time.time() - start_time < timeout:
            session = self.get_session(session_id)
            
            # Check if session has outputs (PR created)
            outputs = session.get("outputs", [])
            if outputs:
                for output in outputs:
                    if "pullRequest" in output:
                        print(f"PR created: {output['pullRequest']['url']}")
                        return session
            
            # Check latest activity
            activities = self.list_activities(session_id, page_size=1)
            if activities.get("activities"):
                latest = activities["activities"][0]
                if "sessionCompleted" in latest:
                    print("Session completed!")
                    return session
                
                # Print progress
                if "progressUpdated" in latest:
                    title = latest["progressUpdated"].get("title", "")
                    print(f"Progress: {title}")
            
            time.sleep(interval)
        
        raise TimeoutError(f"Session monitoring timed out after {timeout} seconds")


def main():
    """Main CLI entry point"""
    parser = argparse.ArgumentParser(description="Jules API Helper")
    parser.add_argument("--api-key", help="Jules API key (or set JULES_API_KEY env var)")
    
    subparsers = parser.add_subparsers(dest="command", help="Command to execute")
    
    # List sources
    subparsers.add_parser("list-sources", help="List available sources")
    
    # Get source
    get_source_parser = subparsers.add_parser("get-source", help="Get source for repository")
    get_source_parser.add_argument("--owner", required=True, help="Repository owner")
    get_source_parser.add_argument("--repo", required=True, help="Repository name")
    
    # Create session
    create_parser = subparsers.add_parser("create-session", help="Create a new session")
    create_parser.add_argument("--source", required=True, help="Source name")
    create_parser.add_argument("--prompt", required=True, help="Task prompt")
    create_parser.add_argument("--title", required=True, help="Session title")
    create_parser.add_argument("--branch", default="main", help="Starting branch")
    create_parser.add_argument("--no-auto-pr", action="store_true", help="Don't auto-create PR")
    
    # Get session
    get_session_parser = subparsers.add_parser("get-session", help="Get session details")
    get_session_parser.add_argument("session_id", help="Session ID")
    
    # List sessions
    subparsers.add_parser("list-sessions", help="List all sessions")
    
    # Monitor session
    monitor_parser = subparsers.add_parser("monitor", help="Monitor session until completion")
    monitor_parser.add_argument("session_id", help="Session ID")
    monitor_parser.add_argument("--interval", type=int, default=30, help="Check interval (seconds)")
    monitor_parser.add_argument("--timeout", type=int, default=3600, help="Timeout (seconds)")
    
    # List activities
    activities_parser = subparsers.add_parser("list-activities", help="List session activities")
    activities_parser.add_argument("session_id", help="Session ID")
    
    args = parser.parse_args()
    
    # Get API key
    api_key = args.api_key or os.environ.get("JULES_API_KEY")
    if not api_key:
        print("Error: API key required. Use --api-key or set JULES_API_KEY environment variable.", file=sys.stderr)
        sys.exit(1)
    
    # Create client
    client = JulesAPIClient(api_key)
    
    # Execute command
    try:
        if args.command == "list-sources":
            result = client.list_sources()
            print(json.dumps(result, indent=2))
        
        elif args.command == "get-source":
            source = client.get_source(args.owner, args.repo)
            if source:
                print(source)
            else:
                print(f"Source not found for {args.owner}/{args.repo}", file=sys.stderr)
                sys.exit(1)
        
        elif args.command == "create-session":
            automation_mode = "MANUAL" if args.no_auto_pr else "AUTO_CREATE_PR"
            result = client.create_session(
                prompt=args.prompt,
                source=args.source,
                title=args.title,
                starting_branch=args.branch,
                automation_mode=automation_mode
            )
            print(json.dumps(result, indent=2))
        
        elif args.command == "get-session":
            result = client.get_session(args.session_id)
            print(json.dumps(result, indent=2))
        
        elif args.command == "list-sessions":
            result = client.list_sessions()
            print(json.dumps(result, indent=2))
        
        elif args.command == "monitor":
            result = client.monitor_session(args.session_id, args.interval, args.timeout)
            print(json.dumps(result, indent=2))
        
        elif args.command == "list-activities":
            result = client.list_activities(args.session_id)
            print(json.dumps(result, indent=2))
        
        else:
            parser.print_help()
            sys.exit(1)
    
    except Exception as e:
        print(f"Error: {e}", file=sys.stderr)
        sys.exit(1)


if __name__ == "__main__":
    main()
