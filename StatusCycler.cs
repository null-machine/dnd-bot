using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

class StatusCycler {
	
	DiscordActivity[] statuses = new DiscordActivity[] {
		new DiscordActivity("mind games", ActivityType.Playing),
		new DiscordActivity("God", ActivityType.Playing),
		new DiscordActivity("with fire", ActivityType.Playing),
		new DiscordActivity("with emotions", ActivityType.Playing),
		new DiscordActivity("DM tears", ActivityType.ListeningTo),
		new DiscordActivity("player tears", ActivityType.ListeningTo),
		new DiscordActivity("bad influences", ActivityType.ListeningTo),
		new DiscordActivity("last words", ActivityType.ListeningTo),
		new DiscordActivity("people lose it", ActivityType.Watching),
		new DiscordActivity("the world burn", ActivityType.Watching)
	};
	
	DiscordClient client;
	Random random;
	int index = 0;
	
	internal StatusCycler(DiscordClient client, Random random) {
		this.client = client;
		this.random = random;
		Cycle().GetAwaiter().GetResult();
	}
	
	async Task Cycle() {
		while (true) {
			int next = (index + random.Next(statuses.Length - 1)) % statuses.Length;
			await client.UpdateStatusAsync(statuses[next]);
			await Task.Delay(300000);
		}
	}
}
