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
		new DiscordActivity("God", ActivityType.Playing),
		new DiscordActivity("with fire", ActivityType.Playing),
		new DiscordActivity("mind games", ActivityType.Playing),

		new DiscordActivity("bad influences", ActivityType.ListeningTo),

		new DiscordActivity("the world burn", ActivityType.Watching),

		new DiscordActivity("shenanigans", ActivityType.Competing),
	};

	DiscordClient client;
	Random random;
	int index;

	internal StatusCycler(DiscordClient client, Random random) {
		this.client = client;
		this.random = random;
		index = random.Next(statuses.Length);
		Cycle().GetAwaiter().GetResult();
	}

	async Task Cycle() {
		while (true) {
			index = (index + random.Next(statuses.Length - 1)) % statuses.Length;
			await client.UpdateStatusAsync(statuses[index], UserStatus.Online);
			await Task.Delay(300000 + random.Next(300000));
		}
	}

	internal async Task Cycle(DiscordClient client, ReadyEventArgs e) {
		index = (index + random.Next(statuses.Length - 1)) % statuses.Length;
		await client.UpdateStatusAsync(statuses[index], UserStatus.Online);
	}
}
